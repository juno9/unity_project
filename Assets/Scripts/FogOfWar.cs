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

        // 1. 먼저 모든 타일을 검은색으로 설정 (거점 제외)
        foreach (var tile in allTiles)
        {
            if (!tile.isBase)
            {
                tile.SetColor(Color.black);
            }
        }

        // 2. 현재 플레이어의 유닛들 주변 시야만 흰색으로 밝힘 (거점 제외)
        HashSet<HexTile> visibleTiles = GetVisibleTiles();
        foreach (var tile in visibleTiles)
        {
            if (!tile.isBase)
            {
                tile.SetColor(Color.white);
            }
        }
    }

    private HashSet<HexTile> GetVisibleTiles()
    {
        HashSet<HexTile> visibleTiles = new HashSet<HexTile>();
        Unit[] allUnits = FindObjectsOfType<Unit>();
        Base[] allBases = FindObjectsOfType<Base>();

        // 유닛 시야 추가
        foreach (Unit unit in allUnits)
        {
            if (unit.playerId == currentPlayer && unit.currentTile != null)
            {
                HashSet<HexTile> tilesInSight = GetTilesInSight(unit.currentTile, unit.sightRange);
                visibleTiles.UnionWith(tilesInSight);
            }
        }

        // 기지 시야 추가
        foreach (Base playerBase in allBases)
        {
            if (playerBase.playerId == currentPlayer && playerBase.currentTile != null)
            {
                HashSet<HexTile> tilesInSight = GetTilesInSight(playerBase.currentTile, playerBase.sightRange);
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
