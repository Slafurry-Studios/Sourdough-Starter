using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PerlinMapGenerator : MonoBehaviour
{
    //  TILES - drag & drop dari Inspector
    [Header("Tilemaps")]
    public Tilemap tilemapFloor;        // Layer paling bawah (floor)
    public Tilemap tilemapWall;         // Layer tengah (wall)
    public Tilemap tilemapDecoration;   // Layer paling atas (dekorasi)

    [Header("Floor Tiles")]
    public Tile[] FloorTiles;           // Bisa 1 atau banyak variasi

    [Header("Wall Tiles")]
    public Tile[] WallTiles;

    [Header("Decoration Tiles")]
    public DecorationEntry[] Decorations; // Tile + kemungkinan muncul (%)

    //  PENGATURAN MAP
    [Header("Map Settings")]
    public int MapWidth = 40;
    public int MapHeight = 40;

    [Range(0f, 1f)]
    public float FloorThreshold = 0.45f;
    public float NoiseScale = 0.1f;
    //  INTERNAL
    public enum Grid { EMPTY, FLOOR, WALL }

    public Grid[,] grid;

    void Start() => GenerateMap();

    void GenerateMap()
    {
        grid = new Grid[MapWidth, MapHeight];

        float ox = Random.Range(0f, 9999f);
        float oy = Random.Range(0f, 9999f);

        // ── Step 1: Buat FLOOR ──────────────────
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                float noise = Mathf.PerlinNoise(x * NoiseScale + ox, y * NoiseScale + oy);

                if (noise > FloorThreshold)
                {
                    grid[x, y] = Grid.FLOOR;
                    tilemapFloor.SetTile(new Vector3Int(x, y, 0), PickRandom(FloorTiles));
                }

                if (x == 0 || x == MapWidth - 1 || y == 0 || y == MapHeight - 1)
                {
                    grid[x, y] = Grid.WALL;
                    tilemapWall.SetTile(new Vector3Int(x, y, 0), PickRandom(WallTiles));
                    continue;
                }
            }
        }

        // ── Step 2: Buat WALL ───────────────────
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        for (int x = 0; x < MapWidth; x++)
        {

            for (int y = 0; y < MapHeight; y++)
            {
                if (grid[x, y] != Grid.FLOOR) continue;

                foreach (var d in dirs)
                {
                    int nx = x + d.x, ny = y + d.y;
                    if (!InBounds(nx, ny) || grid[nx, ny] != Grid.EMPTY) continue;

                    grid[nx, ny] = Grid.WALL;
                    tilemapWall.SetTile(new Vector3Int(nx, ny, 0), PickRandom(WallTiles));
                }
            }
        }

        // ── Step 3: Taruh DEKORASI di atas FLOOR ─
        for (int x = 0; x < MapWidth; x++)
        {

            for (int y = 0; y < MapHeight; y++)
            {
                if (grid[x, y] != Grid.FLOOR) continue;

                foreach (var deco in Decorations)
                {
                    if (Random.value < deco.SpawnChance)
                    {
                        tilemapDecoration.SetTile(new Vector3Int(x, y, 0), deco.Tile);
                        break; // 1 tile dekorasi per cell
                    }
                }
            }
        }
    }

    //  HELPER
    T PickRandom<T>(T[] arr)
    {
        if (arr == null || arr.Length == 0) return default;
        return arr[Random.Range(0, arr.Length)];
    }
    public bool InBounds(int x, int y) => x >= 0 && x < MapWidth && y >= 0 && y < MapHeight;
}

//  Data class untuk dekorasi — muncul di Inspector
[System.Serializable]
public class DecorationEntry
{
    public Tile Tile;

    [Range(0f, 1f)]
    [Tooltip("0 = tidak pernah muncul, 1 = selalu muncul")]
    public float SpawnChance = 0.05f;
}