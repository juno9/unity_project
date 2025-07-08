using UnityEngine;
using System.Collections.Generic;

public class GameInitializer : MonoBehaviour
{
    [Header("유닛 설정")]
    [SerializeField] private GameObject skeletonPrefab; // 스켈레톤 프리팹
    [SerializeField] private bool player1Ranged = false; // 플레이어 1 유닛 타입 (false: 근거리, true: 원거리)
    [SerializeField] private bool player2Ranged = false; // 플레이어 2 유닛 타입 (false: 근거리, true: 원거리)
    
    private HexGrid hexGrid;
    private List<HexTile> availableTiles = new List<HexTile>();
    private List<Unit> spawnedUnits = new List<Unit>();
    
    void Start()
    {
        // HexGrid 찾기
        hexGrid = FindFirstObjectByType<HexGrid>();
        if (hexGrid == null)
        {
            Debug.LogError("HexGrid를 찾을 수 없습니다!");
            return;
        }
        
        // 스켈레톤 프리팹 확인
        if (skeletonPrefab == null)
        {
            Debug.LogError("Skeleton Prefab이 할당되지 않았습니다. Inspector에서 할당해주세요.");
            return;
        }
        
        // 게임 초기화 시작
        StartCoroutine(InitializeGame());
    }
    
    private System.Collections.IEnumerator InitializeGame()
    {
        Debug.Log("게임 초기화 시작...");
        
        // 사용 가능한 타일들 수집
        CollectAvailableTiles();
        
        // 플레이어 1 유닛 1기 배치
        yield return StartCoroutine(SpawnPlayerUnit(1, player1Ranged));
        // 플레이어 2 유닛 1기 배치
        yield return StartCoroutine(SpawnPlayerUnit(2, player2Ranged));
        
        Debug.Log($"게임 초기화 완료! 플레이어 1, 2 각각 1기씩 유닛 생성됨");
        
        // TurnManager 초기화
        InitializeTurnManager();

        // 한 프레임 대기 후 FogOfWar 갱신
        yield return null;
        FogOfWar fogOfWar = FindFirstObjectByType<FogOfWar>();
        if (fogOfWar != null)
        {
            fogOfWar.OnPlayerTurnChanged(1); // 플레이어 1의 턴으로 초기 시야 밝힘
            Debug.Log("GameInitializer가 FogOfWar의 첫 업데이트를 요청했습니다.");
        }
    }
    
    private void CollectAvailableTiles()
    {
        availableTiles.Clear();
        List<HexTile> allTiles = hexGrid.GetAllTiles();
        foreach (HexTile tile in allTiles)
        {
            if (tile != null && !tile.IsOccupied())
            {
                availableTiles.Add(tile);
            }
        }
        Debug.Log($"사용 가능한 타일 수: {availableTiles.Count}");
    }
    
    private System.Collections.IEnumerator SpawnPlayerUnit(int playerId, bool isRanged)
    {
        Color playerColor = playerId == 1 ? Color.blue : new Color(1f, 0.5f, 0f); // 주황색
        HexTile spawnTile = GetRandomSpawnTile();
        if (spawnTile != null)
        {
            SpawnUnit(spawnTile, playerId, playerColor, isRanged);
            yield return null;
        }
        else
        {
            Debug.LogWarning($"플레이어 {playerId}의 유닛을 위한 스폰 위치를 찾을 수 없습니다.");
        }
    }
    
    private HexTile GetRandomSpawnTile()
    {
        if (availableTiles.Count == 0)
            return null;
        int randomIndex = Random.Range(0, availableTiles.Count);
        HexTile selectedTile = availableTiles[randomIndex];
        availableTiles.RemoveAt(randomIndex);
        return selectedTile;
    }
    
    private void SpawnUnit(HexTile tile, int playerId, Color playerColor, bool isRanged)
    {
        // 플레이어 1: 180도 y축 회전(z-), 플레이어 2: 기본(z+)
        Quaternion rotation = playerId == 1 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
        GameObject unitObject = Instantiate(skeletonPrefab, tile.transform.position + Vector3.up * 0.5f, rotation);
        unitObject.name = $"Player{playerId}_Unit_{spawnedUnits.Count + 1}" + (isRanged ? "_Ranged" : "_Melee");
        
        // Unit 컴포넌트 설정
        Unit unit = unitObject.GetComponent<Unit>();
        if (unit == null)
        {
            unit = unitObject.AddComponent<Unit>();
        }
        
        unit.playerId = playerId;
        tile.PlaceUnit(unitObject); // 위치만 맞추고 회전은 그대로 유지
        unit.attackRange = isRanged ? 10 : 1;
        unit.sightRange = isRanged ? 2 : 1; // 원거리: 2, 근거리: 1
        
        // 유닛 색상 변경 (Renderer가 있는 경우)
        Renderer renderer = unitObject.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(renderer.material);
            material.color = playerColor;
            renderer.material = material;
        }
        
        // 타일을 점유 상태로 설정
        tile.SetOccupied(true);
        
        // TurnManager에 유닛 등록
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.RegisterUnit(unit);
        }
        
        spawnedUnits.Add(unit);
        
        Debug.Log($"플레이어 {playerId} {(isRanged ? "원거리" : "근거리")} 유닛 생성됨: {unitObject.name} at {tile.coordinates}");
    }
    
    private void InitializeTurnManager()
    {
        if (TurnManager.Instance != null)
        {
            // TurnManager의 UI 업데이트
            TurnManager.Instance.UpdateButtonColors();
        }
        
        // 유닛 배치 버튼 비활성화 (게임이 시작되었으므로)
        UnitPlacer unitPlacer = FindFirstObjectByType<UnitPlacer>();
        if (unitPlacer != null && unitPlacer.unitPlacementButton != null)
        {
            unitPlacer.unitPlacementButton.gameObject.SetActive(false);
            Debug.Log("유닛 배치 버튼이 비활성화되었습니다. 게임이 시작되었습니다.");
        }
    }
} 