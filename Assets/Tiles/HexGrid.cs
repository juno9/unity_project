using UnityEngine;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour
{
    public GameObject hexTilePrefab;
    [SerializeField] public int mapWidth = 20;    // 기본 맵 가로 크기
    [SerializeField] public int mapHeight = 15;   // 기본 맵 세로 크기
    
    private float hexSize = 1.0f; // 육각형의 반지름 (중심에서 꼭지점까지의 거리)
    private float hexWidth;       // 육각형의 가로 길이
    private float hexHeight;      // 육각형의 세로 길이
    private HexTile[,] tiles;
    private bool isRotating = false;
    private Vector3 lastMousePosition;
    private List<HexTile> tilesList = new List<HexTile>();

    void Start()
    {
        // 육각형의 크기 계산
        hexWidth = hexSize * 2f;                  // 가로 길이 = 반지름 * 2
        hexHeight = hexSize * Mathf.Sqrt(3f);     // 세로 길이 = 반지름 * √3
        
        GenerateGrid();
        // 카메라에 맵 중앙 전달
        CameraController camCtrl = Camera.main != null ? Camera.main.GetComponent<CameraController>() : null;
        if (camCtrl != null)
            camCtrl.mapCenter = GetMapCenter();
    }

    void Update()
    {
        // 우클릭 시작
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
        }
        // 우클릭 끝
        if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
        }

        // 우클릭 중 마우스 이동 시 XY축 회전
        if (isRotating)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float rotationSpeed = 0.3f; // 회전 속도 조절
            // Y축(수직축) 회전 (좌우 드래그)
            transform.Rotate(Vector3.up, delta.x * rotationSpeed, Space.World);
            // X축(좌우축) 회전 (상하 드래그)
            transform.Rotate(Vector3.right, -delta.y * rotationSpeed, Space.World);
            lastMousePosition = Input.mousePosition;
        }
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

        // 모든 타일의 neighbors 리스트 채우기
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                HexTile tile = tiles[x, y];
                if (tile == null) continue;
                tile.neighbors.Clear();
                // 육각형 격자 기준 6방향
                int[][] evenDirs = new int[][] { new[]{+1,0}, new[]{0,-1}, new[]{-1,-1}, new[]{-1,0}, new[]{-1,+1}, new[]{0,+1} };
                int[][] oddDirs  = new int[][] { new[]{+1,0}, new[]{+1,-1}, new[]{0,-1}, new[]{-1,0}, new[]{0,+1}, new[]{+1,+1} };
                int[][] dirs = (x % 2 == 0) ? evenDirs : oddDirs;
                foreach (var d in dirs)
                {
                    int nx = x + d[0];
                    int ny = y + d[1];
                    if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight)
                    {
                        if (tiles[nx, ny] != null)
                            tile.neighbors.Add(tiles[nx, ny]);
                    }
                }
            }
        }
    }

    private GameObject CreateHexTileObject(Vector3 position, float radius, float height = 0.1f)
    {
        GameObject tile = new GameObject("HexTile");
        tile.transform.position = position;

        MeshFilter mf = tile.AddComponent<MeshFilter>();
        MeshRenderer mr = tile.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();

        // 꼭짓점: 윗면 6개, 아랫면 6개, 중심 2개(윗면, 아랫면)
        Vector3[] vertices = new Vector3[14];
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (60 * i);
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            vertices[i] = new Vector3(x, height * 0.5f, z);      // 윗면
            vertices[i + 6] = new Vector3(x, -height * 0.5f, z); // 아랫면
        }
        vertices[12] = new Vector3(0, height * 0.5f, 0);   // 윗면 중심
        vertices[13] = new Vector3(0, -height * 0.5f, 0);  // 아랫면 중심

        // 삼각형 인덱스
        System.Collections.Generic.List<int> triangles = new System.Collections.Generic.List<int>();

        // 윗면
        for (int i = 0; i < 6; i++)
        {
            triangles.Add(12);
            triangles.Add(i);
            triangles.Add((i + 1) % 6);
        }
        // 아랫면
        for (int i = 0; i < 6; i++)
        {
            triangles.Add(13);
            triangles.Add(6 + (i + 1) % 6);
            triangles.Add(6 + i);
        }
        // 옆면
        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;
            triangles.Add(i);
            triangles.Add(6 + i);
            triangles.Add(6 + next);

            triangles.Add(i);
            triangles.Add(6 + next);
            triangles.Add(next);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        mr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mr.material.color = Color.gray;

        var border = tile.AddComponent<TileBorder>();
        border.radius = radius;
        border.transform.localRotation = Quaternion.identity;

        // HexTile 컴포넌트 강제 부착
        var hexTile = tile.GetComponent<HexTile>();
        if (hexTile == null)
            hexTile = tile.AddComponent<HexTile>();
        return tile;
    }

    private void CreateHexTile(Vector2Int coordinates)
    {
        float r = 0.5f;
        float width = r * 2f;
        float height = Mathf.Sqrt(3f) * r;
        float xPos = coordinates.x * width * 0.75f;
        float zPos = coordinates.y * height + (coordinates.x % 2 == 1 ? height / 2f : 0);
        Vector3 position = new Vector3(xPos, 0, zPos);
        GameObject tileObject = CreateHexTileObject(position, r, 0.1f);
        tileObject.name = $"Hex_{coordinates.x}_{coordinates.y}";
        // BoxCollider 추가
        var collider = tileObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(r * 2f, 0.2f, height);
        collider.center = new Vector3(0, 0, 0);
        // HexTile 컴포넌트가 있으면 배열과 리스트에 추가
        HexTile tile = tileObject.GetComponent<HexTile>();
        if (tile != null)
        {
            tile.Initialize(coordinates);
            tiles[coordinates.x, coordinates.y] = tile;
            tilesList.Add(tile);
        }
    }

    public Vector3 GetMapCenter()
    {
        float r = 0.5f; // 타일 반지름
        float width = r * 2f;
        float height = Mathf.Sqrt(3f) * r;
        float centerX = ((mapWidth - 1) * width * 0.75f) / 2f;
        float centerZ = ((mapHeight - 1) * height) / 2f;
        return new Vector3(centerX, 0, centerZ);
    }

    // A* 경로 탐색 함수
    public List<HexTile> FindPath(HexTile start, HexTile goal)
    {
        var openSet = new List<HexTile> { start };
        var cameFrom = new Dictionary<HexTile, HexTile>();
        var gScore = new Dictionary<HexTile, int>();
        var fScore = new Dictionary<HexTile, int>();
        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);
        while (openSet.Count > 0)
        {
            // fScore가 가장 낮은 타일 선택
            HexTile current = openSet[0];
            foreach (var t in openSet)
                if (fScore.ContainsKey(t) && fScore[t] < fScore[current]) current = t;
            if (current == goal)
                return ReconstructPath(cameFrom, current);
            openSet.Remove(current);
            foreach (var neighbor in current.neighbors)
            {
                if (neighbor.unitOnTile != null && neighbor != goal) continue; // 유닛이 있으면 통과 불가
                int tentative_gScore = gScore[current] + 1;
                if (!gScore.ContainsKey(neighbor) || tentative_gScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentative_gScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal);
                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                }
            }
        }
        return null; // 경로 없음
    }

    private int Heuristic(HexTile a, HexTile b)
    {
        // 맨해튼 거리 (육각형 격자)
        return Mathf.Abs(a.coordinates.x - b.coordinates.x) + Mathf.Abs(a.coordinates.y - b.coordinates.y);
    }

    private List<HexTile> ReconstructPath(Dictionary<HexTile, HexTile> cameFrom, HexTile current)
    {
        var totalPath = new List<HexTile> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }

    public HexTile GetTileAt(Vector2Int coords)
    {
        if (tiles == null) return null;
        if (coords.x < 0 || coords.x >= mapWidth || coords.y < 0 || coords.y >= mapHeight) return null;
        return tiles[coords.x, coords.y];
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
} 