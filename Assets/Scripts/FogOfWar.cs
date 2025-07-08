using UnityEngine;
using System.Collections.Generic;

public class FogOfWar : MonoBehaviour
{
    [SerializeField] private float fogUpdateInterval = 0.2f;
    [SerializeField] private bool enableFogOfWar = true;

    private HexGrid hexGrid;
    private float nextUpdateTime;
    private int currentPlayer = 1;

    void Start()
    {
        hexGrid = FindFirstObjectByType<HexGrid>();
        if (hexGrid == null)
        {
            Debug.LogError("HexGrid를 찾을 수 없습니다! FogOfWar 스크립트를 비활성화합니다.");
            this.enabled = false;
            return;
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.RegisterFogOfWar(this);
        }
    }

    void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateFog();
            nextUpdateTime = Time.time + fogUpdateInterval;
        }
    }

    public void OnPlayerTurnChanged(int newPlayer)
    {
        currentPlayer = newPlayer;
        UpdateFog();
    }

    void UpdateFog()
    {
        if (hexGrid == null) return;

        List<HexTile> allTiles = hexGrid.GetAllTiles();

        if (!enableFogOfWar)
        {
            // 안개 비활성화 시 모든 타일을 흰색으로
            foreach (var tile in allTiles)
            {
                tile.SetColor(Color.white);
            }
            return;
        }

        // 1. 먼저 모든 타일을 검은색으로 설정
        foreach (var tile in allTiles)
        {
            tile.SetColor(Color.black);
        }

        // 2. 현재 플레이어의 유닛들 주변 시야만 흰색으로 밝힘
        HashSet<HexTile> visibleTiles = GetVisibleTiles();
        foreach (var tile in visibleTiles)
        {
            tile.SetColor(Color.white);
        }
    }

    // 현재 플레이어의 모든 유닛 시야에 들어오는 타일들을 반환
    private HashSet<HexTile> GetVisibleTiles()
    {
        HashSet<HexTile> visibleTiles = new HashSet<HexTile>();
        Unit[] allUnits = FindObjectsOfType<Unit>();

        foreach (Unit unit in allUnits)
        {
            Debug.Log($"[FogOfWar] 유닛: {unit.name}, currentTile: {unit.currentTile}, sightRange: {unit.sightRange}, playerId: {unit.playerId}, 활성: {unit.gameObject.activeInHierarchy}");
            if (unit.playerId == currentPlayer && unit.currentTile != null)
            {
                // 각 유닛의 시야 범위 내 타일들을 추가 (BFS 사용)
                HashSet<HexTile> tilesInSight = GetTilesInSight(unit.currentTile, unit.sightRange);
                visibleTiles.UnionWith(tilesInSight);
            }
        }
        return visibleTiles;
    }

    // BFS를 사용하여 특정 타일로부터 주어진 범위 내의 모든 타일을 찾음
    private HashSet<HexTile> GetTilesInSight(HexTile startTile, int range)
    {
        HashSet<HexTile> tilesInSight = new HashSet<HexTile>();
        Queue<HexTile> queue = new Queue<HexTile>();
        Dictionary<HexTile, int> distance = new Dictionary<HexTile, int>();

        queue.Enqueue(startTile);
        distance[startTile] = 0;
        tilesInSight.Add(startTile);

        while (queue.Count > 0)
        {
            HexTile current = queue.Dequeue();
            int currentDist = distance[current];

            if (currentDist >= range) continue;

            foreach (HexTile neighbor in current.neighbors)
            {
                if (!distance.ContainsKey(neighbor))
                {
                    distance[neighbor] = currentDist + 1;
                    tilesInSight.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        return tilesInSight;
    }
}
