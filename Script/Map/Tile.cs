using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Dipasang di PREFAB tile. Mendeteksi pointer enter, exit, dan klik pada tile ini,
/// lalu melaporkannya lewat event statis. Yang mengelola logika (hover/active) adalah TileInteraction.
///
/// Syarat agar pointer terdeteksi:
/// 1. Tile punya Collider2D (script ini otomatis menambah BoxCollider2D).
/// 2. Ada EventSystem di scene.
/// 3. Kamera punya komponen Physics2DRaycaster.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Visual")]
    public bool tint = true;

    [Range(0f, 1f)]
    public float hoverSaturation = 0.25f;

    [Range(0f, 1f)]
    public float activeSaturation = 0.50f;

    // Event statis: dilaporkan oleh SEMUA tile, di-subscribe oleh TileInteraction
    public static event Action<Tile> Entered;
    public static event Action<Tile> Exited;
    public static event Action<Tile> Clicked;

    public Vector2Int Coord { get; private set; }
    public bool IsHovered { get; private set; }
    public bool IsActive { get; private set; }

    private SpriteRenderer sr;
    private Color baseColor;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
    }

    /// <summary>Dipanggil GridManager saat tile dibuat.</summary>
    public void Init(int x, int y)
    {
        Coord = new Vector2Int(x, y);
    }

    public void SetHovered(bool value)
    {
        IsHovered = value;
        Refresh();
    }

    public void SetActive(bool value)
    {
        IsActive = value;
        Refresh();
    }

    /// <summary>
    /// Normal = warna asli.
    /// Hover = saturation dinaikkan sedikit.
    /// Active = saturation dinaikkan lebih banyak.
    /// Hue dan brightness tetap.
    /// </summary>
    private void Refresh()
    {
        if (!tint || sr == null) return;

        Color.RGBToHSV(baseColor, out float h, out float s, out float v);

        if (IsActive)
        {
            s = Mathf.Clamp01(s + activeSaturation);
        }
        else if (IsHovered)
        {
            s = Mathf.Clamp01(s + hoverSaturation);
        }

        sr.color = Color.HSVToRGB(h, s, v);
    }

    // ---------- Handler pointer dari EventSystem ----------

    public void OnPointerEnter(PointerEventData eventData)
    {
        Entered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Exited?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke(this);
    }
}
