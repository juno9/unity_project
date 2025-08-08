using UnityEngine;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour
{
    public GameObject hexTilePrefab;
    [SerializeField] private int mapWidth = 20;    // 기본 맵 가로 크기
    [SerializeField] private int mapHeight = 15;   // 기본 맵 세로 크기

    public int width => mapWidth;
    
    private float hexSize = 1.0f; // 육각형의 반지름 (중심에서 꼭지점까지의 거리)
    private float hexWidth;       // 육각형의 가로 길이
    private float hexHeight;      // 육각형의 세로 길이
    private HexTile[,] tiles;

    public int height => mapHeight;

    void Start()
    {
        // 육각형의 크기 계산
        hexWidth = hexSize * 2f;                  // 가로 길이 = 반지름 * 2
        hexHeight = hexSize * Mathf.Sqrt(3f);     // 세로 길이 = 반지름 * √3
        
        GenerateGrid();
        SetAllTileNeighbors();
    }

    public void ResizeGrid(int width, int height)
    {
        mapWidth = width;
        mapHeight = height;
        GenerateGrid();
        SetAllTileNeighbors();
    }

    public void GenerateGrid()
    {
        // Clear existing tiles if any
        if (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        tiles = new HexTile[mapWidth, mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                CreateHexTile(new Vector2Int(x, y));
            }
        }
    }

    private void CreateHexTile(Vector2Int coordinates)
    {
        // Calculate world position for hex tile
        float xPos = coordinates.x * hexWidth * 0.75f;  // 0.75 = 3/4
        float zPos = coordinates.y * hexHeight;

        // Offset every other row
        if (coordinates.y % 2 == 1)
        {
            xPos += hexWidth * 0.375f;  // 0.375 = 3/8
        }

        Vector3 position = new Vector3(xPos, 0, zPos);

        // Create tile
        GameObject tileObject = Instantiate(hexTilePrefab, position, Quaternion.identity, transform);
        HexTile tile = tileObject.GetComponent<HexTile>();
        
        if (tile != null)
        {
            tile.Initialize(coordinates);
            tile.position = position;
            tiles[coordinates.x, coordinates.y] = tile;
            
            // Name the tile for easy identification
            tileObject.name = $"Hex_{coordinates.x}_{coordinates.y}";
        }
    }

    public void SetAllTileNeighbors()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                tiles[x, y].FindNeighbors(this);
            }
        }
    }

    public HexTile GetTileAt(Vector3Int coordinates)
    {
        return GetTileAt(new Vector2Int(coordinates.x, coordinates.y));
    }

    public HexTile GetTileAt(Vector2Int coordinates)
    {
        if (coordinates.x >= 0 && coordinates.x < mapWidth && 
            coordinates.y >= 0 && coordinates.y < mapHeight)
        {
            return tiles[coordinates.x, coordinates.y];
        }
        return null;
    }

    public List<HexTile> GetAllTiles()
    {
        List<HexTile> allTiles = new List<HexTile>();
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (tiles[x, y] != null)
                {
                    allTiles.Add(tiles[x, y]);
                }
            }
        }
        return allTiles;
    }

    public Vector3 GetMapCenter()
    {
        float totalWidth = (mapWidth - 1) * hexWidth * 0.75f;
        float totalHeight = (mapHeight - 1) * hexHeight;
        return new Vector3(totalWidth / 2f, 0, totalHeight / 2f);
    }
} 