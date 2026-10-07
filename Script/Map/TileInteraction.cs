using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Mengamati semua script Tile: menyimpan tile yang sedang di-hover dan tile aktif (active tile).
/// Letakkan di GameObject mana saja (misalnya yang sama dengan GridManager).
/// </summary>
public class TileInteraction : MonoBehaviour
{
    [Header("Options")]
    public bool toggleOnClick = true;      // klik tile aktif lagi = batal aktif
    public bool logEvents = false;         // tampilkan log di Console (untuk debug)

    public Tile HoverTile { get; private set; }
    public Tile ActiveTile { get; private set; }

    // Event untuk script lain
    public event Action<Tile> TileEntered;
    public event Action<Tile> TileExited;
    public event Action<Tile> TileClicked;
    public event Action<Tile> ActiveTileChanged;   // null = tidak ada tile aktif

    private void OnEnable()
    {
        Tile.Entered += HandleEntered;
        Tile.Exited += HandleExited;
        Tile.Clicked += HandleClicked;
    }

    private void OnDisable()
    {
        Tile.Entered -= HandleEntered;
        Tile.Exited -= HandleExited;
        Tile.Clicked -= HandleClicked;
    }

    private void Start()
    {
        // Peringatan setup supaya mudah dicari kalau tile tidak merespons
        if (EventSystem.current == null)
            Debug.LogWarning("TileInteraction: tidak ada EventSystem di scene. (GameObject > UI > Event System)", this);

        Camera cam = Camera.main;
        if (cam == null)
            Debug.LogWarning("TileInteraction: tidak ada kamera ber-tag MainCamera.", this);
        else if (cam.GetComponent<Physics2DRaycaster>() == null)
            Debug.LogWarning("TileInteraction: kamera belum punya komponen Physics2DRaycaster.", cam);
    }

    // ---------- Handler dari Tile ----------

    private void HandleEntered(Tile tile)
    {
        HoverTile = tile;
        tile.SetHovered(true);
        if (logEvents) Debug.Log($"Enter {tile.Coord}");
        TileEntered?.Invoke(tile);
    }

    private void HandleExited(Tile tile)
    {
        if (HoverTile == tile) HoverTile = null;
        tile.SetHovered(false);
        if (logEvents) Debug.Log($"Exit {tile.Coord}");
        TileExited?.Invoke(tile);
    }

    private void HandleClicked(Tile tile)
    {
        if (logEvents) Debug.Log($"Click {tile.Coord}");
        TileClicked?.Invoke(tile);
        SetActiveTile(tile);
    }

    // ---------- Active tile ----------

    public void SetActiveTile(Tile tile)
    {
        if (tile == null) { ClearActiveTile(); return; }

        if (tile == ActiveTile && toggleOnClick)
        {
            ClearActiveTile();
            return;
        }

        if (ActiveTile != null) ActiveTile.SetActive(false);   // != null aman untuk tile yang sudah di-Destroy

        ActiveTile = tile;
        tile.SetActive(true);
        ActiveTileChanged?.Invoke(ActiveTile);
    }

    public void ClearActiveTile()
    {
        if (ActiveTile != null) ActiveTile.SetActive(false);
        ActiveTile = null;
        ActiveTileChanged?.Invoke(null);
    }
}