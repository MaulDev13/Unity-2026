using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grid-based movement controller.
/// The unit occupies exactly one tile at a time and steps smoothly from the
/// center of one cell to the center of an adjacent cell (4-directional).
///
/// Default behavior: random wandering. On an interval it picks a random free
/// neighbouring cell (up/right/down/left) and walks there. Occupancy is tracked
/// through GridManager so two units never share a tile.
///
/// Set randomMovement (via UnitStat) to false to keep the unit standing still
/// on its tile until something else calls TryStepTo / TryMoveTo.
/// </summary>
public class UMC_Grid : UnitMovementController
{
    [Header("Grid")]
    [Tooltip("Grid yang dipakai. Kosong = cari GridManager pertama di scene.")]
    public GridManager grid;

    [Tooltip("Boleh bergerak diagonal (8 arah) selain 4 arah dasar.")]
    public bool allowDiagonal = false;

    [Tooltip("Snap langsung ke pusat tile saat start (tanpa animasi).")]
    public bool snapOnStart = true;

    // Current logical cell the unit owns.
    public Vector2Int CurrentCell { get; private set; }

    // Movement state.
    private bool isMoving;
    private Vector3 moveFrom;
    private Vector3 moveTo;
    private Vector2Int targetCell;
    private float moveProgress; // 0..1 along the current step

    private static readonly Vector2Int[] Dirs4 =
    {
        new Vector2Int(0, 1),   // up
        new Vector2Int(1, 0),   // right
        new Vector2Int(0, -1),  // down
        new Vector2Int(-1, 0),  // left
    };

    private static readonly Vector2Int[] Dirs8 =
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
    };

    public override void Init(UnitController controller)
    {
        base.Init(controller);

        if (grid == null)
            grid = FindObjectOfType<GridManager>();

        if (grid == null)
        {
            Debug.LogError("UMC_Grid: tidak ada GridManager di scene.", this);
            return;
        }

        // Figure out which cell we start on from the current world position.
        if (grid.WorldToGrid(transform.position, out int sx, out int sy))
        {
            CurrentCell = new Vector2Int(sx, sy);
        }
        else
        {
            // Outside the grid: clamp to the nearest valid cell (0,0 as fallback).
            CurrentCell = new Vector2Int(
                Mathf.Clamp(sx, 0, grid.columns - 1),
                Mathf.Clamp(sy, 0, grid.rows - 1));
        }

        // Claim the starting cell. If taken, search for the nearest free one.
        if (!grid.TryOccupy(CurrentCell, unit))
        {
            if (FindNearestFreeCell(CurrentCell, out Vector2Int free))
            {
                CurrentCell = free;
                grid.TryOccupy(CurrentCell, unit);
            }
        }

        if (snapOnStart)
            transform.position = grid.GridToWorld(CurrentCell.x, CurrentCell.y);
    }

    public override void Tick()
    {
        if (grid == null) return;

        if (isMoving)
        {
            AdvanceStep();
            return;
        }

        if (!randomMovement) return;

        timer += Time.deltaTime;
        if (timer >= directionChangeTimer)
        {
            directionChangeTimer += directionChangeInterval;
            TryStepRandom();
        }
    }

    // ---------- Public movement API ----------

    /// <summary>Coba melangkah 1 cell ke arah tertentu (relatif cell sekarang).</summary>
    public bool TryStep(Vector2Int direction)
    {
        return TryStepTo(CurrentCell + direction);
    }

    /// <summary>
    /// Coba melangkah ke cell tujuan (harus bersebelahan, di dalam grid, dan kosong).
    /// Reservasi tujuan segera agar unit lain tidak mengambilnya.
    /// </summary>
    public bool TryStepTo(Vector2Int cell)
    {
        if (isMoving || grid == null) return false;
        if (!grid.IsInside(cell)) return false;
        if (!grid.IsCellFree(cell)) return false;

        // Must be adjacent (prevents teleporting across the board).
        Vector2Int d = cell - CurrentCell;
        bool adjacent = Mathf.Abs(d.x) <= 1 && Mathf.Abs(d.y) <= 1 && d != Vector2Int.zero;
        if (!allowDiagonal && d.x != 0 && d.y != 0) adjacent = false;
        if (!adjacent) return false;

        // Reserve the destination now; keep ownership of the current cell until we arrive.
        if (!grid.TryOccupy(cell, unit)) return false;
        // TryOccupy vacated our old cell, so re-claim it so no one slips in behind us.
        grid.TryOccupy(CurrentCell, unit);
        // NOTE: a unit may transiently "own" two cells mid-step; that is intentional
        // so the trailing cell stays blocked until the step finishes.
        occupiedBoth = true;

        targetCell = cell;
        moveFrom = transform.position;
        moveTo = grid.GridToWorld(cell.x, cell.y);
        moveProgress = 0f;
        isMoving = true;
        return true;
    }

    // Tracks the transient two-cell ownership during a step.
    private bool occupiedBoth;

    private void AdvanceStep()
    {
        float speed = unit != null && unit.Stat != null ? unit.Stat.MoveSpeed : 5f;
        float distance = Vector3.Distance(moveFrom, moveTo);

        if (distance <= 0.0001f)
        {
            ArriveAtTarget();
            return;
        }

        moveProgress += (speed * Time.deltaTime) / distance;
        transform.position = Vector3.Lerp(moveFrom, moveTo, Mathf.Clamp01(moveProgress));

        if (rotationSpeed > 0f)
        {
            Vector2 dir = (moveTo - moveFrom);
            if (dir.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                Quaternion target = Quaternion.Euler(0f, 0f, angle);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, target, rotationSpeed * Time.deltaTime);
            }
        }

        if (moveProgress >= 1f)
            ArriveAtTarget();
    }

    private void ArriveAtTarget()
    {
        transform.position = moveTo;

        // Release the old cell, keep the destination.
        if (occupiedBoth)
        {
            grid.Vacate(CurrentCell, unit);
            occupiedBoth = false;
        }

        CurrentCell = targetCell;
        grid.TryOccupy(CurrentCell, unit);
        isMoving = false;
    }

    private void TryStepRandom()
    {
        Vector2Int[] dirs = allowDiagonal ? Dirs8 : Dirs4;

        // Collect currently valid moves, then pick one at random.
        List<Vector2Int> options = new List<Vector2Int>();
        foreach (Vector2Int d in dirs)
        {
            Vector2Int cell = CurrentCell + d;
            if (grid.IsInside(cell) && grid.IsCellFree(cell))
                options.Add(cell);
        }

        if (options.Count == 0) return; // boxed in; stay put this tick

        Vector2Int chosen = options[Random.Range(0, options.Count)];
        TryStepTo(chosen);
    }

    private bool FindNearestFreeCell(Vector2Int origin, out Vector2Int result)
    {
        // Simple BFS outward from origin to find the closest free cell.
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> seen = new HashSet<Vector2Int>();
        queue.Enqueue(origin);
        seen.Add(origin);

        while (queue.Count > 0)
        {
            Vector2Int c = queue.Dequeue();
            if (grid.IsCellFree(c)) { result = c; return true; }

            foreach (Vector2Int d in Dirs4)
            {
                Vector2Int n = c + d;
                if (grid.IsInside(n) && seen.Add(n))
                    queue.Enqueue(n);
            }
        }

        result = origin;
        return false;
    }

    protected override void OnDeath(UnitController deadUnit)
    {
        // Free the tile so other units can move onto it.
        if (grid != null)
            grid.VacateUnit(unit);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (grid != null)
            grid.VacateUnit(unit);
    }
}
