using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generator random map dengan Wave Function Collapse (WFC).
/// Memakai ukuran grid dari GridManager (columns, rows, cellSize).
/// Pasang di GameObject yang sama dengan GridManager (atau drag referensinya).
/// </summary>
[RequireComponent(typeof(GridManager))]
public class MapGenerator : MonoBehaviour
{
    [Header("Referensi")]
    public GridManager grid;
    public TileData[] tileSet;             // semua jenis tile yang dipakai

    [Header("Aturan")]
    [Tooltip("Jika A.right berisi B, maka B.left otomatis berisi A (dan sebaliknya).")]
    public bool autoSymmetric = true;

    [Header("Random")]
    public bool useRandomSeed = true;
    public int seed = 0;
    [Min(1)] public int maxAttempts = 50;  // ulang jika terjadi kontradiksi

    [Header("Options")]
    public bool generateOnStart = true;

    // Arah: 0=atas, 1=kanan, 2=bawah, 3=kiri
    private static readonly int[] DX = { 0, 1, 0, -1 };
    private static readonly int[] DY = { 1, 0, -1, 0 };
    private static int Opposite(int d) { return (d + 2) % 4; }

    private int tileCount;
    private bool[][][] allowed;            // allowed[dir][a][b] : b boleh di sisi 'dir' dari a
    private bool[][] hasAnyNeighbor;       // hasAnyNeighbor[dir][a] : a boleh punya tetangga di sisi dir
    private bool[,][] possible;            // kemungkinan tile tiap cell
    private int[,] counts;
    private System.Random rng;

    private void Reset()
    {
        grid = GetComponent<GridManager>();
    }

    private void Start()
    {
        if (generateOnStart) Generate();
    }

    [ContextMenu("Generate Map")]
    public void Generate()
    {
        if (grid == null) grid = GetComponent<GridManager>();
        if (tileSet == null || tileSet.Length == 0)
        {
            Debug.LogError("MapGenerator: tileSet masih kosong!", this);
            return;
        }

        BuildRules();

        if (useRandomSeed) seed = System.Environment.TickCount;
        rng = new System.Random(seed);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (TrySolve())
            {
                Render();
                Debug.Log($"MapGenerator: berhasil pada percobaan ke-{attempt} (seed {seed}).");
                return;
            }
            // ganti state random supaya percobaan berikutnya berbeda
            rng = new System.Random(rng.Next());
        }

        Debug.LogError("MapGenerator: gagal menghasilkan map. Aturan tetangga mungkin terlalu ketat/bertentangan.", this);
    }

    // ---------- Persiapan aturan ----------

    private void BuildRules()
    {
        tileCount = tileSet.Length;

        var indexOf = new Dictionary<TileData, int>();
        for (int i = 0; i < tileCount; i++) indexOf[tileSet[i]] = i;

        allowed = new bool[4][][];
        for (int d = 0; d < 4; d++)
        {
            allowed[d] = new bool[tileCount][];
            for (int a = 0; a < tileCount; a++) allowed[d][a] = new bool[tileCount];
        }

        for (int a = 0; a < tileCount; a++)
        {
            for (int d = 0; d < 4; d++)
            {
                foreach (TileData other in tileSet[a].GetList(d))
                {
                    if (other == null || !indexOf.TryGetValue(other, out int b)) continue;
                    allowed[d][a][b] = true;
                    if (autoSymmetric) allowed[Opposite(d)][b][a] = true;
                }
            }
        }

        hasAnyNeighbor = new bool[4][];
        for (int d = 0; d < 4; d++)
        {
            hasAnyNeighbor[d] = new bool[tileCount];
            for (int a = 0; a < tileCount; a++)
                for (int b = 0; b < tileCount; b++)
                    if (allowed[d][a][b]) { hasAnyNeighbor[d][a] = true; break; }
        }
    }

    // ---------- Wave Function Collapse ----------

    private bool TrySolve()
    {
        int w = grid.columns, h = grid.rows;
        possible = new bool[w, h][];
        counts = new int[w, h];

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                possible[x, y] = new bool[tileCount];
                for (int t = 0; t < tileCount; t++) possible[x, y][t] = true;
                counts[x, y] = tileCount;
            }
        }

        var stack = new Stack<Vector2Int>();

        // Aturan tepi: sisi tanpa daftar tetangga hanya boleh menghadap tepi map.
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                bool changed = false;
                for (int d = 0; d < 4; d++)
                {
                    if (grid.IsInside(x + DX[d], y + DY[d]))
                    {
                        for (int t = 0; t < tileCount; t++)
                        {
                            if (possible[x, y][t] && !hasAnyNeighbor[d][t])
                            {
                                possible[x, y][t] = false;
                                counts[x, y]--;
                                changed = true;
                            }
                        }
                    }
                }
                if (counts[x, y] == 0) return false;
                if (changed) stack.Push(new Vector2Int(x, y));
            }
        }
        if (!Propagate(stack)) return false;

        // Loop collapse
        while (true)
        {
            if (!FindLowestEntropy(out int cx, out int cy)) return true; // semua sudah pasti

            int chosen = PickWeighted(cx, cy);
            for (int t = 0; t < tileCount; t++) possible[cx, cy][t] = (t == chosen);
            counts[cx, cy] = 1;

            stack.Push(new Vector2Int(cx, cy));
            if (!Propagate(stack)) return false;
        }
    }

    private bool Propagate(Stack<Vector2Int> stack)
    {
        while (stack.Count > 0)
        {
            Vector2Int c = stack.Pop();

            for (int d = 0; d < 4; d++)
            {
                int nx = c.x + DX[d], ny = c.y + DY[d];
                if (!grid.IsInside(nx, ny)) continue;

                bool neighborChanged = false;

                for (int t = 0; t < tileCount; t++)
                {
                    if (!possible[nx, ny][t]) continue;

                    // t valid jika ada tile s di cell c yang mengizinkan t di sisi d
                    bool supported = false;
                    for (int s = 0; s < tileCount; s++)
                    {
                        if (possible[c.x, c.y][s] && allowed[d][s][t]) { supported = true; break; }
                    }

                    if (!supported)
                    {
                        possible[nx, ny][t] = false;
                        counts[nx, ny]--;
                        neighborChanged = true;
                    }
                }

                if (counts[nx, ny] == 0) return false;
                if (neighborChanged) stack.Push(new Vector2Int(nx, ny));
            }
        }
        return true;
    }

    /// <summary>Cari cell dengan pilihan paling sedikit (>1). False jika semua sudah ditentukan.</summary>
    private bool FindLowestEntropy(out int bx, out int by)
    {
        bx = by = -1;
        int best = int.MaxValue;
        int ties = 0;

        for (int x = 0; x < grid.columns; x++)
        {
            for (int y = 0; y < grid.rows; y++)
            {
                int c = counts[x, y];
                if (c <= 1) continue;

                if (c < best) { best = c; bx = x; by = y; ties = 1; }
                else if (c == best)
                {
                    ties++;
                    if (rng.Next(ties) == 0) { bx = x; by = y; } // pilih acak di antara yang sama
                }
            }
        }
        return bx >= 0;
    }

    private int PickWeighted(int x, int y)
    {
        float total = 0f;
        for (int t = 0; t < tileCount; t++)
            if (possible[x, y][t]) total += tileSet[t].weight;

        float r = (float)rng.NextDouble() * total;
        int last = 0;
        for (int t = 0; t < tileCount; t++)
        {
            if (!possible[x, y][t]) continue;
            last = t;
            r -= tileSet[t].weight;
            if (r <= 0f) return t;
        }
        return last;
    }

    // ---------- Render ke scene ----------

    private void Render()
    {
        grid.PrepareGrid();

        for (int x = 0; x < grid.columns; x++)
        {
            for (int y = 0; y < grid.rows; y++)
            {
                for (int t = 0; t < tileCount; t++)
                {
                    if (possible[x, y][t])
                    {
                        grid.PlaceTile(x, y, tileSet[t].prefab);
                        break;
                    }
                }
            }
        }
    }
}