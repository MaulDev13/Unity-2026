using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grid system dinamis untuk layout map.
/// Tile diambil dari prefab (sprite square ukuran 1x1 unit).
/// Pasang script ini pada GameObject kosong di scene.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [Min(1)] public int columns = 10;          // jumlah cell horizontal (X)
    [Min(1)] public int rows = 10;             // jumlah cell vertikal (Y)
    [Min(0.01f)] public float cellSize = 1f;   // ukuran 1 cell (world unit)
    [Min(0f)] public float spacing = 0f;       // jarak antar cell (opsional)

    [Header("Tile")]
    public GameObject tilePrefab;              // prefab sprite square 1x1
    public Transform tileParent;               // parent tile (opsional, default: object ini)

    [Header("Options")]
    public bool centerGrid = true;             // true = grid di tengah object, false = mulai dari pojok kiri-bawah
    public bool generateOnStart = true;

    private GameObject[,] tiles;

    // Occupancy: which unit currently stands on a given cell.
    private readonly Dictionary<Vector2Int, UnitController> occupants = new Dictionary<Vector2Int, UnitController>();

    // Jarak antar pusat 2 cell yang bersebelahan
    private float Step => cellSize + spacing;

    private void Start()
    {
        if (generateOnStart) GenerateGrid();
    }

    /// <summary>Buat grid baru berdasarkan variabel saat ini.</summary>
    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("GridManager: tilePrefab belum diisi!", this);
            return;
        }

        ClearGrid();

        if (tileParent == null) tileParent = transform;
        tiles = new GameObject[columns, rows];

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector3 pos = GridToWorld(x, y);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, tileParent);

                // Sprite 1x1 -> scale = cellSize agar ukuran sesuai
                tile.transform.localScale = new Vector3(cellSize, cellSize, 1f);
                tile.name = $"Tile_{x}_{y}";
                InitTile(tile, x, y);

                tiles[x, y] = tile;
            }
        }
    }

    /// <summary>Kosongkan grid dan siapkan array tile (dipakai MapGenerator).</summary>
    public void PrepareGrid()
    {
        if (tileParent == null) tileParent = transform;
        ClearGrid();
        tiles = new GameObject[columns, rows];
    }

    /// <summary>Taruh prefab tertentu pada cell (x,y). Dipanggil setelah PrepareGrid().</summary>
    public GameObject PlaceTile(int x, int y, GameObject prefab)
    {
        if (tiles == null || !IsInside(x, y) || prefab == null) return null;

        GameObject tile = Instantiate(prefab, GridToWorld(x, y), Quaternion.identity, tileParent);
        tile.transform.localScale = new Vector3(cellSize, cellSize, 1f);
        tile.name = $"Tile_{x}_{y}_{prefab.name}";
        InitTile(tile, x, y);
        tiles[x, y] = tile;
        return tile;
    }

    /// <summary>Hapus semua tile yang sudah dibuat.</summary>
    [ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
        Transform parent = tileParent != null ? tileParent : transform;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }

        tiles = null;
        occupants.Clear();
    }

    /// <summary>Ubah ukuran grid saat runtime.</summary>
    public void Resize(int newColumns, int newRows, float newCellSize)
    {
        columns = Mathf.Max(1, newColumns);
        rows = Mathf.Max(1, newRows);
        cellSize = Mathf.Max(0.01f, newCellSize);
        GenerateGrid();
    }

    /// <summary>Beri tahu script Tile (jika ada) koordinat grid-nya.</summary>
    private void InitTile(GameObject tileObject, int x, int y)
    {
        Tile tile = tileObject.GetComponent<Tile>();
        if (tile != null) tile.Init(x, y);
    }

    // ---------- Konversi koordinat ----------

    /// <summary>Koordinat grid (x,y) -> posisi world (pusat cell).</summary>
    public Vector3 GridToWorld(int x, int y)
    {
        Vector3 offset = Vector3.zero;
        if (centerGrid)
        {
            offset = new Vector3(
                -(columns - 1) * Step * 0.5f,
                -(rows - 1) * Step * 0.5f,
                0f);
        }
        return transform.position + offset + new Vector3(x * Step, y * Step, 0f);
    }

    /// <summary>Posisi world -> koordinat grid. Return false jika di luar grid.</summary>
    public bool WorldToGrid(Vector3 worldPos, out int x, out int y)
    {
        Vector3 origin = GridToWorld(0, 0);
        x = Mathf.RoundToInt((worldPos.x - origin.x) / Step);
        y = Mathf.RoundToInt((worldPos.y - origin.y) / Step);
        return IsInside(x, y);
    }

    public bool IsInside(int x, int y)
    {
        return x >= 0 && x < columns && y >= 0 && y < rows;
    }

    public bool IsInside(Vector2Int cell) => IsInside(cell.x, cell.y);

    // ---------- Occupancy (unit menempati cell) ----------

    /// <summary>Apakah cell bisa ditempati: di dalam grid dan belum ada unit lain.</summary>
    public bool IsCellFree(Vector2Int cell)
    {
        if (!IsInside(cell)) return false;
        return !occupants.TryGetValue(cell, out UnitController u) || u == null;
    }

    /// <summary>Unit yang sedang berada di cell (null jika kosong / di luar grid).</summary>
    public UnitController GetOccupant(Vector2Int cell)
    {
        if (!IsInside(cell)) return null;
        return occupants.TryGetValue(cell, out UnitController u) ? u : null;
    }

    /// <summary>
    /// Daftarkan unit pada cell. Return false jika di luar grid atau cell sudah
    /// ditempati unit lain. Jika unit sudah terdaftar di cell lain, pindahkan.
    /// </summary>
    public bool TryOccupy(Vector2Int cell, UnitController unit)
    {
        if (unit == null || !IsInside(cell)) return false;

        if (occupants.TryGetValue(cell, out UnitController current) && current != null && current != unit)
            return false;

        // Lepas registrasi lama unit ini (kalau ada) agar tidak menempati 2 cell.
        VacateUnit(unit);

        occupants[cell] = unit;
        return true;
    }

    /// <summary>Kosongkan cell jika yang menempati memang unit ini.</summary>
    public void Vacate(Vector2Int cell, UnitController unit)
    {
        if (occupants.TryGetValue(cell, out UnitController current) && current == unit)
            occupants.Remove(cell);
    }

    /// <summary>Hapus unit dari cell mana pun yang ia tempati.</summary>
    public void VacateUnit(UnitController unit)
    {
        if (unit == null) return;

        Vector2Int? found = null;
        foreach (KeyValuePair<Vector2Int, UnitController> kv in occupants)
        {
            if (kv.Value == unit) { found = kv.Key; break; }
        }

        if (found.HasValue) occupants.Remove(found.Value);
    }

    /// <summary>Ambil tile pada koordinat grid (null jika di luar grid).</summary>
    public GameObject GetTile(int x, int y)
    {
        if (tiles == null || !IsInside(x, y)) return null;
        return tiles[x, y];
    }

    // ---------- Preview di Scene view ----------

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 1f, 0.35f);
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Gizmos.DrawWireCube(GridToWorld(x, y), new Vector3(cellSize, cellSize, 0f));
            }
        }
    }
}