using UnityEngine;
using System.Collections.Generic;

public class GameInitializer : MonoBehaviour
{
    [Header("유닛 설정")]
    [SerializeField] private GameObject skeletonPrefab; // 스켈레톤 프리팹
    [SerializeField] private bool player1Ranged = false; // 플레이어 1 유닛 타입 (false: 근거리, true: 원거리)
    [SerializeField] private bool player2Ranged = false; // 플레이어 2 유닛 타입 (false: 근거리, true: 원거리)
    
    [Header("기지 설정")]
    [SerializeField] private GameObject basePrefab; // 기지 프리팹 (없으면 빈 오브젝트로 생성)

    private HexGrid hexGrid;
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
        
        // 플레이어 1 거점 및 유닛 배치
        yield return StartCoroutine(SpawnBaseAndUnit(1, player1Ranged));
        // 플레이어 2 거점 및 유닛 배치
        yield return StartCoroutine(SpawnBaseAndUnit(2, player2Ranged));

        Debug.Log($"게임 초기화 완료! 플레이어 1, 2 거점 및 유닛 생성됨");
        
        // TurnManager를 통해 첫 턴 시작
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.StartFirstTurn();
        }
        // Base 생성이 완료된 후 카메라 초기 시점 설정
        Debug.Log($"[Camera Log] GameInitializer: Bases should be spawned now. Checking for Base objects before camera transition.");
        Base[] basesBeforeTransition = FindObjectsByType<Base>(FindObjectsSortMode.None);
        Debug.Log($"[Camera Log] GameInitializer: Found {basesBeforeTransition.Length} Base objects before calling TransitionToPlayerView.");
        
        CameraController cameraController = FindFirstObjectByType<CameraController>();
        if (cameraController != null)
        {
            Base player1Base = null;
            Base[] bases = FindObjectsByType<Base>(FindObjectsSortMode.None);
            foreach (Base b in bases)
            {
                if (b.playerId == 1)
                {
                    player1Base = b;
                    break;
                }
            }

            if (player1Base != null)
            {
                cameraController.TransitionToPlayerView(player1Base.transform.position, 1);
            }
        }
        
        // FogOfWar 업데이트
        FogOfWar fogOfWar = FindFirstObjectByType<FogOfWar>();
        if (fogOfWar != null)
        {
            fogOfWar.OnPlayerTurnChanged(1); // 첫 턴은 플레이어 1
        }
    }
    
    private System.Collections.IEnumerator SpawnBaseAndUnit(int playerId, bool isRanged)
    {
        Color playerColor = playerId == 1 ? Color.blue : new Color(1f, 0.5f, 0f); // 주황색

        Vector2Int spawnCoordinates;
        int minX, maxX, minY, maxY;

        // Define spawn regions for each player
        if (playerId == 1)
        {
            minX = 0;
            maxX = hexGrid.mapWidth / 2 - 1;
            minY = 0;
            maxY = hexGrid.mapHeight - 1;
        }
        else // playerId == 2
        {
            minX = hexGrid.mapWidth / 2;
            maxX = hexGrid.mapWidth - 1;
            minY = 0;
            maxY = hexGrid.mapHeight - 1;
        }

        // Find a random unoccupied tile within the designated area
        HexTile foundTile = null;
        int attempts = 0;
        int maxAttempts = 100;

        while (foundTile == null && attempts < maxAttempts)
        {
            int randomX = Random.Range(minX, maxX + 1); // +1 because Random.Range is exclusive for int max
            int randomY = Random.Range(minY, maxY + 1);
            Vector2Int potentialCoords = new Vector2Int(randomX, randomY);
            
            HexTile tile = hexGrid.GetTileAt(potentialCoords);
            if (tile != null && !tile.IsOccupied()) // Use IsOccupied() method
            {
                foundTile = tile;
            }
            attempts++;
        }

        if (foundTile != null)
        {
            spawnCoordinates = foundTile.coordinates;
        }
        else
        {
            Debug.LogWarning($"Could not find a random spawn tile for Player {playerId} after {maxAttempts} attempts. Using default.");
            // Fallback to a fixed position if random fails
            spawnCoordinates = playerId == 1 ? new Vector2Int(0, 0) : new Vector2Int(hexGrid.mapWidth - 1, hexGrid.mapHeight - 1);
        }

        HexTile spawnTile = hexGrid.GetTileAt(spawnCoordinates);

        if (spawnTile != null)
        {
            // 기지 생성 및 배치
            GameObject baseObject;
            if (basePrefab != null)
            {
                baseObject = Instantiate(basePrefab, spawnTile.transform.position + Vector3.up * 0.1f, Quaternion.identity);
                baseObject.name = $"Player{playerId}_Base";
            }
            else
            {
                baseObject = new GameObject($"Player{playerId}_Base");
                baseObject.transform.position = spawnTile.transform.position + Vector3.up * 0.1f;
            }
            
            Base playerBase = baseObject.AddComponent<Base>();
            playerBase.playerId = playerId;
            playerBase.currentTile = spawnTile;
            spawnTile.baseOnTile = playerBase;

            // 기지에 LineRenderer 추가
            if (baseObject.GetComponent<LineRenderer>() == null)
            {
                baseObject.AddComponent<LineRenderer>();
            }

            // 기지 타일 색상 변경 (녹색)
            Debug.Log($"[GameInitializer] 플레이어 {playerId} 기지 타일 색상 변경 시도: {spawnTile.coordinates} -> Green");
            spawnTile.SetColor(Color.green);
            Debug.Log($"[GameInitializer] 플레이어 {playerId} 기지 생성됨: {baseObject.name} at {spawnTile.coordinates}. 현재 타일 색상: {spawnTile.GetComponent<MeshRenderer>().material.color}");

            // 기지 타일 테두리 색상 변경
            TileBorder tileBorder = spawnTile.GetComponent<TileBorder>();
            if (tileBorder != null)
            {
                tileBorder.SetBorderColor(playerColor);
                Debug.Log($"[GameInitializer] 플레이어 {playerId} 기지 타일 테두리 색상 변경 완료: {playerColor}");
            }

            SpawnUnit(spawnTile, playerId, playerColor, isRanged);
            yield return null;
        }
        else
        {
            Debug.LogWarning($"플레이어 {playerId}의 유닛을 위한 스폰 위치를 찾을 수 없습니다. 좌표: {spawnCoordinates}");
        }
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
    
    
} 