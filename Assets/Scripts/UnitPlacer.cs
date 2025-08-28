using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UnitPlacer : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab; // 생성할 유닛 프리팹을 Inspector에서 할당

    [Header("Player Materials")]
    [SerializeField] private Material player1Material;
    [SerializeField] private Material player2Material;

    public HexGrid hexGrid;
    private bool isPlacing = false;
    private bool isAttacking = false; // 공격 모드 추가
    private HexTile lastHighlightedTile = null;
    private Unit selectedUnit = null;
    private List<HexTile> attackRangeTiles = new List<HexTile>(); // 공격 범위 타일들
    
    // 커서 관련 변수 추가
    [SerializeField] private Texture2D normalCursor;
    [SerializeField] private Texture2D attackCursor;
    private Vector2 cursorHotspot = Vector2.zero;
    
    // 플레이어 관련 변수 추가
    private int currentPlayer = 1; // 1 또는 2
    private Color player1Color = new Color(0.2f, 0.6f, 1f, 1f); // 파란색
    private Color player2Color = new Color(1f, 0.5f, 0f, 1f); // 주황색
    public Button unitPlacementButton; // 버튼을 public으로 변경
    public Button attackButton; // 공격 버튼 추가
    public Button attackMoveButton; // 공격 이동 버튼 추가
    private bool isMoving = false; // 이동 모드 추가
    private bool isAttackMoving = false; // 공격 이동 모드 추가
    private List<HexTile> moveRangeTiles = new List<HexTile>(); // 이동 범위 타일들
    private bool isRangedPlacing = false;
    public Button rangedUnitPlacementButton;
    private bool isAttackCursorSet = false;
    private Unit hoveredAttackTarget = null;
    

    void Start()
    {
        
        
        // 기본 커서 설정
        SetNormalCursor();
        
        // TurnManager가 없으면 생성
        if (TurnManager.Instance == null)
        {
            GameObject turnManagerObj = new GameObject("TurnManager");
            turnManagerObj.AddComponent<TurnManager>();
        }

        // DamageText가 없으면 생성
        if (DamageText.Instance == null)
        {
            GameObject damageTextObj = new GameObject("DamageText");
            damageTextObj.AddComponent<DamageText>();
        }

        if (unitPrefab == null)
        {
            Debug.LogError("Unit Prefab이 할당되지 않았습니다. Inspector에서 Unit Prefab을 할당해주세요.");
            return;
        }

        if (hexGrid == null)
        {
            hexGrid = FindFirstObjectByType<HexGrid>();
        }
        // 유닛 배치 버튼 동적 생성 및 연결
        CreateUnitPlacementButton();
        CreateRangedUnitPlacementButton();
        CreateAttackButton();
        CreateAttackMoveButton();
    }

    private void CreateAttackMoveButton()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject buttonObj = new GameObject("AttackMoveButton");
        buttonObj.transform.SetParent(canvas.transform);
        attackMoveButton = buttonObj.AddComponent<Button>();
        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(1f, 0.5f, 0.2f, 1f); // 주황색 계열
        RectTransform rt = buttonObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(75, 35);
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -280); // 이동 버튼 아래

        // 텍스트 추가
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        Text text = textObj.AddComponent<Text>();
        text.text = "공격 이동";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        // 버튼 클릭 이벤트 연결
        attackMoveButton.onClick.AddListener(StartAttackMove);
        attackMoveButton.gameObject.SetActive(false); // 초기에는 비활성화
    }

    private void CreateUnitPlacementButton()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        GameObject buttonObj = new GameObject("creatingUnit");
        buttonObj.transform.SetParent(canvas.transform);
        unitPlacementButton = buttonObj.AddComponent<Button>();
        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 1f, 1f);
        RectTransform rt = buttonObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(75, 35);
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -20); // 우측 상단에서 약간 안쪽
        // 텍스트 추가
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        Text text = textObj.AddComponent<Text>();
        text.text = "유닛 배치";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        // 버튼 클릭 이벤트 연결
        unitPlacementButton.onClick.AddListener(StartPlacement);
    }

    private void CreateRangedUnitPlacementButton()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject buttonObj = new GameObject("RangedUnitButton");
        buttonObj.transform.SetParent(canvas.transform);
        rangedUnitPlacementButton = buttonObj.AddComponent<Button>();
        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.8f, 0.5f, 1f, 1f); // 보라색 계열
        RectTransform rt = buttonObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(75, 35);
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -100); // 기존 유닛 배치 버튼 아래

        // 텍스트 추가
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        Text text = textObj.AddComponent<Text>();
        text.text = "원거리 유닛 배치";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        rangedUnitPlacementButton.onClick.AddListener(StartRangedPlacement);
    }

    private void CreateAttackButton()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject buttonObj = new GameObject("AttackButton");
        buttonObj.transform.SetParent(canvas.transform);
        attackButton = buttonObj.AddComponent<Button>();
        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(1f, 0.3f, 0.3f, 1f); // 빨간색
        RectTransform rt = buttonObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(75, 35);
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -240); // 유닛 배치 버튼 아래

        // 텍스트 추가
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        Text text = textObj.AddComponent<Text>();
        text.text = "공격";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        // 버튼 클릭 이벤트 연결
        attackButton.onClick.AddListener(StartAttack);
        attackButton.gameObject.SetActive(false); // 초기에는 비활성화
    }

    private void ShowAttackButton()
    {
        if (attackButton == null) return;
        if (selectedUnit != null && !selectedUnit.hasAttacked)
        {
            attackButton.gameObject.SetActive(true);
        }
        else
        {
            attackButton.gameObject.SetActive(false);
        }
    }

    private void ShowActionButtons()
    {
        if (selectedUnit == null) return;
        // 공격 버튼만 표시
        if (attackButton != null)
        {
            attackButton.gameObject.SetActive(!selectedUnit.hasAttacked);
        }
        if (attackMoveButton != null)
        {
            attackMoveButton.gameObject.SetActive(!selectedUnit.hasMoved);
        }
    }

    private void StartAttack()
    {
        
        if (selectedUnit == null || selectedUnit.hasAttacked)
        {
            
            return;
        }
        // 이동 모드/이전 공격 모드 등 모두 취소
        CancelMove();
        CancelAttack();
        SetNormalCursor();
        isAttacking = true;
        SetAttackCursor(); // 공격 커서로 변경
        ShowAttackRange();
        
    }

    private void CancelAttack()
    {
        
        isAttacking = false;
        HideAttackRange();
        if (attackButton != null)
        {
            attackButton.gameObject.SetActive(false);
        }
        
    }

    private void ShowAttackRange()
    {
        if (selectedUnit == null) return;

        HideAttackRange(); // 기존 범위 숨기기

        List<Unit> opponentUnits = TurnManager.Instance.GetOpponentUnits(selectedUnit.playerId);
        if (opponentUnits == null) return;

        foreach (Unit opponent in opponentUnits)
        {
            if (opponent == null || opponent.currentTile == null) continue;

            if (selectedUnit.CanAttack(opponent))
            {
                opponent.currentTile.SetHighlight(new Color(1f, 0.2f, 0.2f, 0.7f)); // 반투명 빨간색
                attackRangeTiles.Add(opponent.currentTile);
            }
        }
    }

    private void HideAttackRange()
    {
        foreach (HexTile tile in attackRangeTiles)
        {
            if (tile != null)
            {
                tile.ResetHighlight();
            }
        }
        attackRangeTiles.Clear();
    }

    private void HandleAttackClick(HexTile clickedTile)
    {
        // isAttacking 확인 로직을 제거하여 컨텍스트 메뉴(우클릭)를 통한 공격을 허용합니다.
        if (selectedUnit == null || clickedTile == null) return;

        Unit targetUnit = clickedTile.unitOnTile;
        if (targetUnit != null && targetUnit.playerId != selectedUnit.playerId)
        {
            if (selectedUnit.CanAttack(targetUnit))
            {
                selectedUnit.Attack(targetUnit);
                TurnManager.Instance.UpdateFogOfWar(); // 공격 후 안개 갱신
                DeselectAndCancel(); // 상태 초기화
            }
        }
    }

    private void PlaceUnit(HexTile tile)
    {
        if (tile == null)
        {
            Debug.LogError("PlaceUnit: 전달된 tile이 null입니다.");
            return;
        }
        var gridTile = hexGrid.GetTileAt(tile.coordinates);
        if (gridTile == null)
        {
            Debug.LogError($"PlaceUnit: hexGrid.GetTileAt로 얻은 tile이 null입니다. coordinates: {tile.coordinates}");
            return;
        }
        tile = gridTile;
        if (tile.unitOnTile != null)
        {
            
            return;
        }
        if (tile.unitOnTile == null)
        {
            GameObject newUnit = Instantiate(unitPrefab, tile.transform.position, Quaternion.identity);
            newUnit.transform.SetParent(tile.transform);
            newUnit.name = $"Unit_{tile.coordinates.x}_{tile.coordinates.y}";
            
            Unit unit = newUnit.GetComponent<Unit>();
            if (unit == null)
            {
                unit = newUnit.AddComponent<Unit>();
            }
            unit.playerId = TurnManager.Instance.currentPlayer;
            unit.attackRange = isRangedPlacing ? 10 : 1; // 원거리/근거리 구분

            // 플레이어 ID에 따라 다른 머티리얼 적용
            var renderer = newUnit.GetComponentInChildren<SkinnedMeshRenderer>();
            if (renderer != null)
            {
                if (unit.playerId == 1 && player1Material != null)
                {
                    renderer.material = player1Material;
                }
                else if (unit.playerId == 2 && player2Material != null)
                {
                    renderer.material = player2Material;
                }
            }
            else
            {
                Debug.LogWarning($"유닛 '{newUnit.name}'에서 SkinnedMeshRenderer를 찾을 수 없습니다. 플레이어 색상이 적용되지 않습니다.");
            }

            newUnit.SetActive(true);

            // Collider가 없으면 자동으로 BoxCollider 추가
            if (newUnit.GetComponent<Collider>() == null)
            {
                BoxCollider col = newUnit.AddComponent<BoxCollider>();
                // 필요하다면 col.center, col.size 조정 (기본값 사용)
            }

            unit.currentTile = tile;
            tile.unitOnTile = unit;

            // --- 유닛 방향 설정 ---
            Vector3 lookTarget = Vector3.zero;
            List<Unit> opponentUnits = TurnManager.Instance.GetOpponentUnits(unit.playerId);
            if (opponentUnits != null && opponentUnits.Count > 0)
            {
                // 1순위: 가장 가까운 적 유닛
                lookTarget = opponentUnits[0].transform.position; // (개선 제안: 실제 가장 가까운 유닛을 찾도록 로직 추가 가능)
            }
            else
            {
                // 2순위: 상대방 Base
                Base opponentBase = FindPlayerBase(unit.playerId == 1 ? 2 : 1);
                if (opponentBase != null)
                {
                    lookTarget = opponentBase.transform.position;
                }
                else
                {
                    // 3순위: 맵 중앙
                    lookTarget = FindFirstObjectByType<HexGrid>().GetMapCenter();
                }
            }
            newUnit.transform.LookAt(new Vector3(lookTarget.x, newUnit.transform.position.y, lookTarget.z));
            // --- ---

            // 유닛과 거점 연결선 그리기
            Base playerBase = FindPlayerBase(unit.playerId);
            if (playerBase != null)
            {
                LineDrawer lineDrawer = newUnit.GetComponent<LineDrawer>();
                if (lineDrawer == null)
                {
                    lineDrawer = newUnit.AddComponent<LineDrawer>();
                }
                Color lineColor = unit.playerId == 1 ? player1Color : player2Color;
                lineDrawer.DrawLine(newUnit.transform.position, playerBase.transform.position, lineColor);
            }


            
            TurnManager.Instance.RegisterUnit(unit);
            isRangedPlacing = false; // 배치 후 리셋

            // 유닛 배치 후 안개 갱신
            TurnManager.Instance.UpdateFogOfWar();

            
        }
    }

    private Base FindPlayerBase(int playerId)
    {
        Base[] bases = FindObjectsByType<Base>(FindObjectsSortMode.None);
        foreach (Base b in bases)
        {
            if (b.playerId == playerId)
            {
                return b;
            }
        }
        return null;
    }

    private void MoveUnit(HexTile targetTile)
    {
        if (selectedUnit == null) {  return; }
        HexTile currentTile = hexGrid.GetTileAt(selectedUnit.currentTile.coordinates);
        targetTile = hexGrid.GetTileAt(targetTile.coordinates);
        if (currentTile == null) {  return; }
        if (targetTile == null) {  return; }
        if (targetTile.unitOnTile != null) {  return; }
        
        
        var path = hexGrid.FindPath(currentTile, targetTile);
        if (path == null) {  return; }
        if (path.Count < 2) {  return; }
        
        currentTile.unitOnTile = null;
        StartCoroutine(MoveUnitAlongPath(selectedUnit, path, targetTile));
    }

    private IEnumerator MoveUnitAlongPath(Unit unit, List<HexTile> path, HexTile targetTile)
    {
        
        Animator animator = unit.GetComponentInChildren<Animator>();
        if (animator != null) animator.SetBool("isWalking", true);
        for (int i = 1; i < path.Count; i++)
        {
            Vector3 start = unit.transform.position;
            Vector3 end = path[i].transform.position;
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 3f; // 속도 조절
                unit.transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }
            unit.transform.position = end;
        }
        if (animator != null) animator.SetBool("isWalking", false);
        unit.transform.SetParent(targetTile.transform);
        unit.currentTile = targetTile;
        targetTile.unitOnTile = unit;
        unit.hasMoved = true;

        // 유닛 이동 후 선 다시 그리기
        Base playerBase = FindPlayerBase(unit.playerId);
        if (playerBase != null)
        {
            LineDrawer lineDrawer = unit.GetComponent<LineDrawer>();
            if (lineDrawer != null)
            {
                Color lineColor = unit.playerId == 1 ? player1Color : player2Color;
                lineDrawer.DrawLine(unit.transform.position, playerBase.transform.position, lineColor);
            }
        }

        selectedUnit = null;
        

        // 유닛 이동 후 안개 갱신
        TurnManager.Instance.UpdateFogOfWar();

        
    }

    public void StartPlacement()
    {
        
        isPlacing = true;
        // 기존 UI 숨기기
        if (unitPlacementButton != null) unitPlacementButton.gameObject.SetActive(false);
        if (rangedUnitPlacementButton != null) rangedUnitPlacementButton.gameObject.SetActive(false);
        if (attackButton != null) attackButton.gameObject.SetActive(false);
        // 안내문구 표시
        ShowGuideText("유닛을 배치할 타일을 선택해 주세요");
    }

    public void CancelPlacement()
    {
        isPlacing = false;
        if (lastHighlightedTile != null)
        {
            lastHighlightedTile.ResetHighlight();
            lastHighlightedTile = null;
        }
        // 기존 UI 복구
        if (unitPlacementButton != null) unitPlacementButton.gameObject.SetActive(true);
        if (rangedUnitPlacementButton != null) rangedUnitPlacementButton.gameObject.SetActive(true);
        if (attackButton != null) attackButton.gameObject.SetActive(true);
        // 안내문구 숨기기
        HideGuideText();
    }

    private void StartRangedPlacement()
    {
        isPlacing = true;
        isRangedPlacing = true;
        // 기존 StartPlacement와 동일하게 동작
    }

    // 커서 변경 메서드들
    private void SetNormalCursor()
    {
        try
        {
            if (normalCursor != null)
            {
                Cursor.SetCursor(normalCursor, cursorHotspot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"커서 복원 중 오류: {e.Message}");
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    private void SetAttackCursor()
    {
        try
        {
            if (attackCursor != null)
            {
                Cursor.SetCursor(attackCursor, cursorHotspot, CursorMode.Auto);
            }
            else
            {
                // 기본 커서가 없으면 빨간색 원형 커서 생성
                CreateDefaultAttackCursor();
            }
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"공격 커서 설정 중 오류: {e.Message}");
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    private void CreateDefaultAttackCursor()
    {
        // 32x32 크기의 빨간색 원형 커서 생성
        Texture2D cursorTex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[32 * 32];
        
        Vector2 center = new Vector2(16, 16);
        float radius = 12f;
        
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    pixels[y * 32 + x] = Color.red;
                }
                else
                {
                    pixels[y * 32 + x] = Color.clear;
                }
            }
        }
        
        cursorTex.SetPixels(pixels);
        cursorTex.Apply();
        
        // 텍스처를 읽기 가능하게 설정
        cursorTex.filterMode = FilterMode.Point;
        cursorTex.wrapMode = TextureWrapMode.Clamp;
        
        Cursor.SetCursor(cursorTex, center, CursorMode.Auto);
    }

    private void CancelMove()
    {
        
        isMoving = false;
        HideMoveRange();
        
    }

    private void ShowMoveRange()
    {
        if (selectedUnit == null || selectedUnit.currentTile == null) return;

        HideMoveRange(); // 기존 범위 숨기기

        // 이동 범위 내의 모든 타일 찾기
        foreach (HexTile tile in hexGrid.GetAllTiles())
        {
            if (tile == null) continue;
            
            // 빈 타일만 이동 가능
            if (tile.unitOnTile == null)
            {
                int distance = selectedUnit.currentTile.GetDistanceTo(tile);
                if (distance <= selectedUnit.moveRange && distance > 0)
                {
                    // 플레이어 색상으로 하이라이트
                    Color playerColor = selectedUnit.playerId == 1 ? player1Color : player2Color;
                    tile.SetHighlight(playerColor);
                    moveRangeTiles.Add(tile);
                }
            }
        }
    }

    private void HideMoveRange()
    {
        foreach (HexTile tile in moveRangeTiles)
        {
            if (tile != null)
            {
                tile.ResetHighlight();
            }
        }
        moveRangeTiles.Clear();
    }

    private void ShowGuideText(string message)
    {
        if (TurnManager.Instance != null && TurnManager.Instance.guideText != null)
        {
            TurnManager.Instance.guideText.text = message;
            TurnManager.Instance.guideText.gameObject.SetActive(true);
        }
    }

    private void HideGuideText()
    {
        if (TurnManager.Instance != null && TurnManager.Instance.guideText != null)
        {
            TurnManager.Instance.guideText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (TurnManager.Instance == null || (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()))
            return;

        // --- Right-click Action (Process input from this frame) ---
        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClickAction();
            return; // Right-click action taken, so skip other logic for this frame.
        }

        // --- Left-click Action (Process input from this frame) ---
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            HandleLeftClick(hit);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            // Clicked on empty space, deselect everything.
            DeselectAndCancel();
        }

        // --- Cursor State Update (Prepare for NEXT frame) ---
        HandleCursorState();
    }

    private void HandleCursorState()
    {
        hoveredAttackTarget = null; // 매번 초기화

        if (selectedUnit == null || selectedUnit.playerId != TurnManager.Instance.currentPlayer)
        {
            if (isAttackCursorSet)
            {
                SetNormalCursor();
                isAttackCursorSet = false;
            }
            return;
        }

        bool shouldBeAttackCursor = false;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Unit targetUnit = hit.collider.GetComponentInParent<Unit>();
            if (targetUnit != null && targetUnit.playerId != selectedUnit.playerId && !selectedUnit.hasAttacked && selectedUnit.CanAttack(targetUnit))
            {
                shouldBeAttackCursor = true;
                hoveredAttackTarget = targetUnit; // 유효한 공격 대상 저장
            }
        }

        if (shouldBeAttackCursor && !isAttackCursorSet)
        {
            SetAttackCursor();
            isAttackCursorSet = true;
        }
        else if (!shouldBeAttackCursor && isAttackCursorSet)
        {
            SetNormalCursor();
            isAttackCursorSet = false;
        }
    }

    private void HandleLeftClick(RaycastHit hit)
    {
        Unit hitUnit = hit.collider.GetComponentInParent<Unit>();
        Base hitBase = hit.collider.GetComponentInParent<Base>();
        HexTile hitTile = hit.collider.GetComponent<HexTile>();

        if (Input.GetMouseButtonDown(0))
        {
            if (isPlacing)
            {
                if (hitTile != null)
                {
                    if (TurnManager.Instance.SpendAP(TurnManager.UNIT_PLACEMENT_COST))
                    {
                        PlaceUnit(hitTile);
                    }
                    else
                    {
                        ShowGuideText("AP가 부족하여 유닛을 배치할 수 없습니다.");
                    }
                    CancelPlacement();
                }
                return;
            }

            if (hitBase != null)
            {
                DeselectAndCancel();
                TurnManager.Instance.ShowBaseInfo(hitBase);
            }
            else if (hitUnit != null)
            {
                // Clicked on a unit
                if (isAttacking && selectedUnit != null && selectedUnit.playerId != hitUnit.playerId && selectedUnit.CanAttack(hitUnit))
                {
                    // If in attack mode and clicked a valid enemy, attack it.
                    HandleAttackClick(hitUnit.currentTile);
                }
                else if (hitUnit.playerId == TurnManager.Instance.currentPlayer)
                {
                    // Clicked a friendly unit, select it.
                    SelectUnit(hitUnit);
                }
                else
                {
                    // Clicked an enemy unit without being in attack mode, just show info.
                    DeselectAndCancel();
                    TurnManager.Instance.ShowUnitInfo(hitUnit);
                }
            }
            else if (hitTile != null)
            {
                // Clicked on a tile
                if (selectedUnit != null && isMoving && !selectedUnit.hasMoved && moveRangeTiles.Contains(hitTile) && hitTile.unitOnTile == null)
                {
                    // A unit is selected and the clicked tile is in move range and empty.
                    if (TurnManager.Instance.SpendAP(TurnManager.MOVE_COST))
                    {
                        MoveUnit(hitTile);
                        DeselectAndCancel();
                    }
                    else
                    {
                        ShowGuideText("AP가 부족하여 이동할 수 없습니다.");
                    }
                }
                else if (isAttacking && selectedUnit != null)
                {
                    // If in attack mode, and the tile has an enemy, attack it.
                    if (hitTile.unitOnTile != null && hitTile.unitOnTile.playerId != selectedUnit.playerId)
                    {
                        HandleAttackClick(hitTile);
                    }
                    else
                    {
                        // Clicked an empty tile while in attack mode, just deselect.
                        DeselectAndCancel();
                    }
                }
                else
                {
                    // Clicked an empty tile with no unit selected, or out of range.
                    DeselectAndCancel();
                }
            }
        }
    }

    private void HandleRightClickAction()
    {
        if (selectedUnit == null)
        {
            DeselectAndCancel();
            return;
        }

        // 마우스 오버로 확인된 공격 대상이 있으면 즉시 공격
        if (hoveredAttackTarget != null && !selectedUnit.hasAttacked)
        {
            HandleAttackClick(hoveredAttackTarget.currentTile);
            DeselectAndCancel();
            return;
        }

        // 공격 대상이 없으면 이동 로직 처리
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (isMoving && !selectedUnit.hasMoved)
            {
                HexTile clickedTile = hit.collider.GetComponent<HexTile>();
                if (clickedTile != null && moveRangeTiles.Contains(clickedTile) && clickedTile.unitOnTile == null)
                {
                    if (TurnManager.Instance.SpendAP(TurnManager.MOVE_COST))
                    {
                        MoveUnit(clickedTile);
                        DeselectAndCancel();
                        return;
                    }
                    else
                    {
                        ShowGuideText("AP가 부족하여 이동할 수 없습니다.");
                        return;
                    }
                }
            }
        }

        // 유효한 행동이 아니면 선택 취소
        DeselectAndCancel();
    }

    private void SelectUnit(Unit unit)
    {
        DeselectAndCancel(); // Clear previous state first.

        selectedUnit = unit;
        TurnManager.Instance.ShowUnitInfo(unit);
        ShowActionButtons();

        // 유닛이 아직 움직이지 않았다면 자동으로 이동 모드로 전환하고 이동 범위를 표시합니다.
        if (!selectedUnit.hasMoved)
        {
            isMoving = true;
            ShowMoveRange();
        }
    }

    private void DeselectAndCancel()
    {
        selectedUnit = null;
        CancelPlacement();
        CancelAttack();
        CancelMove();
        CancelAttackMove();
        SetNormalCursor();
        TurnManager.Instance.ShowUnitInfo(null);
        HideGuideText();
    }


    private void StartAttackMove()
    {
        
        if (selectedUnit == null || selectedUnit.hasMoved)
        {
            
            return;
        }
        // 다른 모드 모두 취소
        CancelAttack();
        CancelMove();
        SetNormalCursor();
        isAttackMoving = true;
        ShowMoveRange();
        
    }

    private void CancelAttackMove()
    {
        
        isAttackMoving = false;
        HideMoveRange();
        if (attackMoveButton != null)
        {
            attackMoveButton.gameObject.SetActive(false);
        }
        
    }

    private void HandleAttackMoveClick(HexTile clickedTile)
    {
        if (selectedUnit == null || !isAttackMoving || clickedTile == null) return;

        

        if (clickedTile.unitOnTile == null)
        {
            int distance = selectedUnit.currentTile.GetDistanceTo(clickedTile);
            if (distance <= selectedUnit.moveRange && distance > 0)
            {
                if (TurnManager.Instance.SpendAP(TurnManager.ATTACK_MOVE_COST))
                {
                    
                    AttackMoveUnit(clickedTile);
                    CancelAttackMove();
                    selectedUnit = null;
                    TurnManager.Instance.ShowUnitInfo(null);
                }
                else
                {
                    ShowGuideText("AP가 부족하여 공격 이동을 할 수 없습니다.");
                }
            }
            else
            {
                
            }
        }
        else
        {
            
        }
    }

    private void AttackMoveUnit(HexTile targetTile)
    {
        if (selectedUnit == null) {  return; }
        HexTile currentTile = hexGrid.GetTileAt(selectedUnit.currentTile.coordinates);
        targetTile = hexGrid.GetTileAt(targetTile.coordinates);
        if (currentTile == null) {  return; }
        if (targetTile == null) {  return; }
        if (targetTile.unitOnTile != null) {  return; }
        var path = hexGrid.FindPath(currentTile, targetTile);
        if (path == null) {  return; }
        if (path.Count < 2) {  return; }
        currentTile.unitOnTile = null;
        StartCoroutine(AttackMoveUnitAlongPath(selectedUnit, path, targetTile));
    }

    private IEnumerator AttackMoveUnitAlongPath(Unit unit, List<HexTile> path, HexTile targetTile)
    {
        
        Animator animator = unit.GetComponentInChildren<Animator>();
        if (animator != null) animator.SetBool("isWalking", true);

        for (int i = 1; i < path.Count; i++)
        {
            // 이동 전 시야 내 적 탐색
            List<Unit> opponentUnits = TurnManager.Instance.GetOpponentUnits(unit.playerId);
            foreach (Unit opponent in opponentUnits)
            {
                if (unit.GetDistanceToUnit(opponent) <= unit.sightRange)
                {
                    if (unit.CanAttack(opponent))
                    {
                        unit.Attack(opponent);
                        if (animator != null) animator.SetBool("isWalking", false);
                        yield break; // 공격 후 이동 중지
                    }
                }
            }

            Vector3 start = unit.transform.position;
            Vector3 end = path[i].transform.position;
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 3f; // 속도 조절
                unit.transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }
            unit.transform.position = end;
        }

        if (animator != null) animator.SetBool("isWalking", false);
        unit.transform.SetParent(targetTile.transform);
        unit.currentTile = targetTile;
        targetTile.unitOnTile = unit;
        unit.hasMoved = true;

        Base playerBase = FindPlayerBase(unit.playerId);
        if (playerBase != null)
        {
            LineDrawer lineDrawer = unit.GetComponent<LineDrawer>();
            if (lineDrawer != null)
            {
                Color lineColor = unit.playerId == 1 ? player1Color : player2Color;
                lineDrawer.DrawLine(unit.transform.position, playerBase.transform.position, lineColor);
            }
        }

        TurnManager.Instance.UpdateFogOfWar();
    }

    
}
