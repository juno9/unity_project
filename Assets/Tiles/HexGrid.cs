using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public GameObject hexTilePrefab;
    [SerializeField] public int mapWidth = 20;    // 기본 맵 가로 크기
    [SerializeField] public int mapHeight = 15;   // 기본 맵 세로 크기
    
    private float hexSize = 1.0f; // 육각형의 반지름 (중심에서 꼭지점까지의 거리)
    private float hexWidth;       // 육각형의 가로 길이
    private float hexHeight;      // 육각형의 세로 길이
    private HexTile[,] tiles;

    void Start()
    {
        // 육각형의 크기 계산
        hexWidth = hexSize * 2f;                  // 가로 길이 = 반지름 * 2
        hexHeight = hexSize * Mathf.Sqrt(3f);     // 세로 길이 = 반지름 * √3
        
        GenerateGrid();
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
        float r = 0.5f; // 육각형의 반지름 (프리팹의 Scale.x/2)
        float width = r * 2f;
        float height = Mathf.Sqrt(3f) * r;

        float xPos = coordinates.x * width * 0.75f;
        float zPos = coordinates.y * height + (coordinates.x % 2 == 1 ? height / 2f : 0);

        Vector3 position = new Vector3(xPos, 0, zPos);

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
} 