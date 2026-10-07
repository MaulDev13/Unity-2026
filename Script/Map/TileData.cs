using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data 1 jenis tile + aturan tetangga untuk tiap sisi.
/// Buat lewat: Assets > Create > Map > Tile Data
///
/// ATURAN:
/// - Isi daftar "boleh bertetangga" di tiap sisi.
/// - Tile yang TIDAK ada di daftar = tidak boleh jadi tetangga di sisi itu.
/// - Daftar KOSONG pada suatu sisi = sisi itu hanya boleh berada di TEPI MAP
///   (tidak boleh bersebelahan dengan tile apa pun).
/// </summary>
[CreateAssetMenu(fileName = "NewTile", menuName = "Map/Tile Data")]
public class TileData : ScriptableObject
{
    [Header("Visual")]
    public GameObject prefab;              // prefab sprite square 1x1

    [Header("Kemunculan")]
    [Min(0.0001f)] public float weight = 1f;   // makin besar, makin sering muncul

    [Header("Tetangga yang diizinkan (kosong = hanya di tepi map)")]
    public List<TileData> up = new List<TileData>();
    public List<TileData> right = new List<TileData>();
    public List<TileData> down = new List<TileData>();
    public List<TileData> left = new List<TileData>();

    /// <summary>Ambil daftar berdasarkan arah: 0=atas, 1=kanan, 2=bawah, 3=kiri.</summary>
    public List<TileData> GetList(int dir)
    {
        switch (dir)
        {
            case 0: return up;
            case 1: return right;
            case 2: return down;
            default: return left;
        }
    }
}