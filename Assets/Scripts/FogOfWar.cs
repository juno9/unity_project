using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class FogOfWar : MonoBehaviour
{
    [SerializeField] private Material fogMaterial;
    [SerializeField] private float fogRadius = 3f; // 유닛의 시야 범위
    [SerializeField] private float fogUpdateInterval = 0.5f; // 안개 업데이트 주기
    [SerializeField] private int mapWidth = 20; // 맵의 가로 타일 수 (HexGrid와 맞추세요)
    [SerializeField] private int mapHeight = 20; // 맵의 세로 타일 수 (HexGrid와 맞추세요)
    [SerializeField] private bool enableFogOfWar = true; // 전장의 안개 활성화/비활성화
    
    private float nextUpdateTime;
    private GameObject fogPlane;
    private int[,] fogState; // 0: 미탐색, 1: 전장의 안개, 2: 밝은 시야
    private Texture2D fogTexture;
    private Texture2D fogTexture2D;
    private Color colorUnseen = new Color(0,0,0,1); // 검은 안개
    private Color colorExplored = new Color(0.2f,0.2f,0.2f,0.7f); // 전장의 안개(회색)
    private Color colorVisible = new Color(0,0,0,0); // 밝은 시야(투명)
    private Camera mainCam;
    private float fogPlaneDistance = 10f; // 카메라에서 얼마나 떨어진 곳에 Fog Plane을 둘지
    private int currentPlayer = 1; // 현재 플레이어

    void Start()
    {
        mainCam = Camera.main;
        fogState = new int[mapWidth, mapHeight];
        fogTexture = new Texture2D(mapWidth, mapHeight, TextureFormat.RGBA32, false);
        fogTexture.filterMode = FilterMode.Point;
        if (fogMaterial != null)
            fogMaterial.SetTexture("_FogTexture", fogTexture);
        CreateFogPlaneForCamera();
        EnsureUnitExistsOrSpawn();
        
        // TurnManager에 등록
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.RegisterFogOfWar(this);
        }
    }

    void CreateFogPlaneForCamera()
    {
        if (fogPlane != null) Destroy(fogPlane);
        Camera cam = Camera.main;
        float distance = 2.5f; // camDistance(2) + 여유
        Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0, 0, distance));
        Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1, 1, distance));
        Vector3 center = (bl + tr) / 2f;
        Vector3 size = tr - bl;
        fogPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fogPlane.name = "FogOfWarPlane";
        fogPlane.transform.position = center;
        fogPlane.transform.localScale = new Vector3(size.x, size.y, 1);
        fogPlane.transform.rotation = Quaternion.LookRotation(cam.transform.forward, cam.transform.up);
        fogPlane.transform.SetParent(cam.transform); // 카메라의 자식으로!
        var renderer = fogPlane.GetComponent<MeshRenderer>();
        if (renderer != null && fogMaterial != null)
            renderer.material = fogMaterial;
        fogPlane.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    void EnsureUnitExistsOrSpawn()
    {
        if (FindObjectsOfType<Unit>().Length == 0)
        {
            // 맵 중앙 또는 랜덤 위치에 유닛 생성
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject unitPrefab = Resources.Load<GameObject>("Skeleton"); // Resources 폴더에 프리팹 필요
            if (unitPrefab != null)
            {
                Instantiate(unitPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Resources/Skeleton 프리팹이 필요합니다.");
            }
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        // 카메라 뷰 내 랜덤 위치
        if (mainCam == null) mainCam = Camera.main;
        Vector3 randViewport = new Vector3(Random.Range(0.2f, 0.8f), Random.Range(0.2f, 0.8f), fogPlaneDistance-1f);
        return mainCam.ViewportToWorldPoint(randViewport);
    }

    void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateFog();
            nextUpdateTime = Time.time + fogUpdateInterval;
        }
    }

    // 현재 플레이어 변경 시 호출
    public void OnPlayerTurnChanged(int newPlayer)
    {
        currentPlayer = newPlayer;
        Debug.Log($"FogOfWar: 플레이어 {currentPlayer}의 턴으로 변경됨");
        UpdateFog(); // 즉시 안개 업데이트
    }

    void UpdateFog()
    {
        if (!enableFogOfWar)
        {
            // 안개 비활성화 시 모든 타일을 밝게
            for (int x = 0; x < mapWidth; x++)
                for (int y = 0; y < mapHeight; y++)
                    fogState[x, y] = 2;
            UpdateFogTexture();
            return;
        }

        // 1. 이전 밝음(2) → 전장의 안개(1)로 낮춤
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                if (fogState[x, y] == 2) fogState[x, y] = 1;
        
        // 2. 현재 플레이어의 유닛들만 시야 계산
        Unit[] allUnits = FindObjectsOfType<Unit>();
        foreach (Unit unit in allUnits)
        {
            if (unit.playerId == currentPlayer) // 현재 플레이어의 유닛만
            {
                foreach (Vector2Int tile in GetHexTilesInSight(unit))
                {
                    if (tile.x >= 0 && tile.x < mapWidth && tile.y >= 0 && tile.y < mapHeight)
                        fogState[tile.x, tile.y] = 2;
                }
            }
        }
        UpdateFogTexture();
    }

    void UpdateFogTexture()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float a = 1f; // 미탐색(검정)
                if (fogState[x, y] == 1) a = 0.7f; // 전장의 안개(회색)
                if (fogState[x, y] == 2) a = 0f;   // 밝은 시야(투명)
                fogTexture.SetPixel(x, y, new Color(0, 0, 0, a));
            }
        }
        fogTexture.Apply();
    }

    List<Vector2Int> GetHexTilesInSight(Unit unit)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        if (unit == null || unit.currentTile == null) return result;
        Vector2Int center = unit.currentTile.coordinates;
        int radius = 3; // 기본값
        var sightRangeField = unit.GetType().GetField("sightRange");
        if (sightRangeField != null)
            radius = (int)sightRangeField.GetValue(unit);
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = Mathf.Max(-radius, -dx - radius); dy <= Mathf.Min(radius, -dx + radius); dy++)
            {
                int x = center.x + dx;
                int y = center.y + dy;
                if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                    result.Add(new Vector2Int(x, y));
            }
        }
        return result;
    }

    void OnDestroy()
    {
        if (fogTexture != null)
        {
            Destroy(fogTexture);
        }
        if (fogTexture2D != null)
        {
            Destroy(fogTexture2D);
        }
    }
} 