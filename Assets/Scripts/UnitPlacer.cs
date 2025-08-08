using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UnitPlacer : MonoBehaviour
{
    [SerializeField] private GameObject skeletonPrefab; // 스켈레톤 프리팹을 Inspector에서 할당
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
    private Color player1Color = Color.blue;
    private Color player2Color = new Color(1f, 0.5f, 0f); // 주황색
    public Button unitPlacementButton; // 버튼을 public으로 변경
    public Button attackButton; // 공격 버튼 추가
    public Button moveButton; // 이동 버튼 추가
    public Button attackMoveButton; // 공격 이동 버튼 추가
    private bool isMoving = false; // 이동 모드 추가
    private bool isAttackMoving = false; // 공격 이동 모드 추가
    private List<HexTile> moveRangeTiles = new List<HexTile>(); // 이동 범위 타일들
    private bool isRangedPlacing = false;
    public Button rangedUnitPlacementButton;
    

    void Start()
    {
        Debug.Log("UnitPlacer Start 실행");
        
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

        if (skeletonPrefab == null)
        {
            Debug.LogError("Skeleton Prefab이 할당되지 않았습니다. Inspector에서 Skeleton Prefab을 할당해주세요.");
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
        CreateMoveButton();
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

    private void CreateMoveButton()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject buttonObj = new GameObject("MoveButton");
        buttonObj.transform.SetParent(canvas.transform);
        moveButton = buttonObj.AddComponent<Button>();
        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.3f, 1f, 0.3f, 1f); // 초록색
        RectTransform rt = buttonObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(75, 35);
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -190); // 공격 버튼 아래

        // 텍스트 추가
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        Text text = textObj.AddComponent<Text>();
        text.text = "단순 이동";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        // 버튼 클릭 이벤트 연결
        moveButton.onClick.AddListener(StartMove);
        moveButton.gameObject.SetActive(false); // 초기에는 비활성화
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
        // 이동 버튼 표시
        if (moveButton != null)
        {
            moveButton.gameObject.SetActive(!selectedUnit.hasMoved);
        }
        if (attackMoveButton != null)
        {
            attackMoveButton.gameObject.SetActive(!selectedUnit.hasMoved);
        }
    }

    private void StartAttack()
    {
        Debug.Log("StartAttack 호출됨");
        if (selectedUnit == null || selectedUnit.hasAttacked)
        {
            Debug.Log("공격할 수 있는 유닛이 없습니다.");
            return;
        }
        // 이동 모드/이전 공격 모드 등 모두 취소
        CancelMove();
        CancelAttack();
        SetNormalCursor();
        isAttacking = true;
        SetAttackCursor(); // 공격 커서로 변경
        ShowAttackRange();
        Debug.Log("공격 모드 시작 완료");
    }

    private void CancelAttack()
    {
        Debug.Log("CancelAttack 호출됨");
        isAttacking = false;
        HideAttackRange();
        if (attackButton != null)
        {
            attackButton.gameObject.SetActive(false);
        }
        Debug.Log("CancelAttack 완료");
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
        if (selectedUnit == null || !isAttacking || clickedTile == null) return;
        // 클릭된 타일에 적 유닛이 있는지 확인
        if (clickedTile.unitOnTile != null)
        {
            Unit targetUnit = clickedTile.unitOnTile;
            if (targetUnit != null && targetUnit.playerId != selectedUnit.playerId)
            {
                selectedUnit.Attack(targetUnit);
                CancelAttack();
                SetNormalCursor();
                selectedUnit = null;
                TurnManager.Instance.ShowUnitInfo(null);

                // 공격 후 안개 갱신
                TurnManager.Instance.UpdateFogOfWar();

                
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
            Debug.Log("이미 유닛이 배치된 타일입니다.");
            return;
        }
        if (tile.unitOnTile == null)
        {
            GameObject newUnit = Instantiate(skeletonPrefab, tile.transform.position, Quaternion.identity);
            newUnit.transform.SetParent(tile.transform);
            newUnit.name = $"Unit_{tile.coordinates.x}_{tile.coordinates.y}";
            
            Unit unit = newUnit.GetComponent<Unit>();
            if (unit == null)
            {
                unit = newUnit.AddComponent<Unit>();
            }
            unit.playerId = TurnManager.Instance.currentPlayer;
            unit.attackRange = isRangedPlacing ? 10 : 1; // 원거리/근거리 구분

            // URP용 머티리얼 자동 할당 및 플레이어별 텍스처 적용
            Transform geoMesh = newUnit.transform.Find("Geometry/geo/Skeleton");
            if (geoMesh != null)
            {
                var smr = geoMesh.GetComponent<SkinnedMeshRenderer>();
                if (smr != null)
                {
                    Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                    if (urpShader != null)
                    {
                        Material urpMat = new Material(urpShader);
                        // 플레이어별 텍스처 경로 지정 (tga 확장자, 파일명 반영)
                        string texPath = unit.playerId == 1
                            ? "Skeleton/Textures/SceletonVersion2"
                            : "Skeleton/Textures/SceletonVersion3";
                        Texture2D tex = Resources.Load<Texture2D>(texPath);
                        if (tex != null)
                        {
                            urpMat.mainTexture = tex;
                        }
                        else
                        {
                            Debug.LogWarning($"텍스처를 찾을 수 없습니다: {texPath}");
                        }
                        smr.material = urpMat;
                    }
                    else
                    {
                        Debug.LogError("URP용 Shader를 찾을 수 없습니다. Universal Render Pipeline/Lit이 프로젝트에 포함되어 있는지 확인하세요.");
                    }
                }
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

            // --- 상대방 유닛을 바라보게 ---
            List<Unit> opponentUnits = TurnManager.Instance.GetOpponentUnits(unit.playerId);
            if (opponentUnits != null && opponentUnits.Count > 0)
            {
                // 가장 가까운 상대 유닛을 찾거나, 첫 번째 유닛을 기준으로
                Vector3 targetPos = opponentUnits[0].transform.position;
                newUnit.transform.LookAt(new Vector3(targetPos.x, newUnit.transform.position.y, targetPos.z));
            }
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


            Debug.Log($"[배치] {unit.name}의 currentTile: {unit.currentTile != null}, tile: {tile.coordinates}");
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
        if (selectedUnit == null) { Debug.Log("selectedUnit is null"); return; }
        HexTile currentTile = hexGrid.GetTileAt(selectedUnit.currentTile.coordinates);
        targetTile = hexGrid.GetTileAt(targetTile.coordinates);
        if (currentTile == null) { Debug.Log("currentTile is null"); return; }
        if (targetTile == null) { Debug.Log("targetTile is null"); return; }
        if (targetTile.unitOnTile != null) { Debug.Log("targetTile already has a unit"); return; }
        Debug.Log($"currentTile: {currentTile.coordinates}, neighbors: {currentTile.neighbors.Count}");
        Debug.Log($"targetTile: {targetTile.coordinates}, neighbors: {targetTile.neighbors.Count}");
        var path = hexGrid.FindPath(currentTile, targetTile);
        if (path == null) { Debug.Log("No path found"); return; }
        if (path.Count < 2) { Debug.Log("Path too short"); return; }
        Debug.Log("MoveUnit: path found, starting coroutine");
        currentTile.unitOnTile = null;
        StartCoroutine(MoveUnitAlongPath(selectedUnit, path, targetTile));
    }

    private IEnumerator MoveUnitAlongPath(Unit unit, List<HexTile> path, HexTile targetTile)
    {
        Debug.Log("MoveUnitAlongPath started");
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
        Debug.Log($"[이동] {unit.name}의 currentTile: {unit.currentTile != null}, tile: {targetTile.coordinates}");

        // 유닛 이동 후 안개 갱신
        TurnManager.Instance.UpdateFogOfWar();

        
    }

    public void StartPlacement()
    {
        Debug.Log($"플레이어 {TurnManager.Instance.currentPlayer} 턴 시작");
        isPlacing = true;
        // 기존 UI 숨기기
        if (unitPlacementButton != null) unitPlacementButton.gameObject.SetActive(false);
        if (rangedUnitPlacementButton != null) rangedUnitPlacementButton.gameObject.SetActive(false);
        if (attackButton != null) attackButton.gameObject.SetActive(false);
        if (moveButton != null) moveButton.gameObject.SetActive(false);
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
        if (moveButton != null) moveButton.gameObject.SetActive(true);
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
            Debug.Log("커서가 일반 모드로 복원되었습니다.");
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
            Debug.Log("커서가 공격 모드로 변경되었습니다.");
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

    private void StartMove()
    {
        Debug.Log("StartMove 호출됨");
        if (selectedUnit == null || selectedUnit.hasMoved)
        {
            Debug.Log("이동할 수 있는 유닛이 없습니다.");
            return;
        }
        // 공격 모드/이전 이동 모드 등 모두 취소
        CancelAttack();
        CancelMove();
        SetNormalCursor();
        isMoving = true;
        ShowMoveRange();
        Debug.Log("이동 모드 시작 완료");
    }

    private void CancelMove()
    {
        Debug.Log("CancelMove 호출됨");
        isMoving = false;
        HideMoveRange();
        if (moveButton != null)
        {
            moveButton.gameObject.SetActive(false);
        }
        Debug.Log("CancelMove 완료");
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

    private void HandleMoveClick(HexTile clickedTile)
    {
        if (selectedUnit == null || !isMoving || clickedTile == null) return;

        Debug.Log("HandleMoveClick 호출됨");

        // 클릭된 타일이 이동 가능한지 확인
        if (clickedTile.unitOnTile == null)
        {
            int distance = selectedUnit.currentTile.GetDistanceTo(clickedTile);
            if (distance <= selectedUnit.moveRange && distance > 0)
            {
                if (TurnManager.Instance.SpendAP(TurnManager.MOVE_COST))
                {
                    Debug.Log($"이동 시작: {selectedUnit.name} -> {clickedTile.coordinates}");
                    
                    // 이동 실행
                    MoveUnit(clickedTile);
                    
                    Debug.Log("이동 실행 완료, 후처리 시작");
                    
                    // 이동 완료 후 처리
                    CancelMove();
                    Debug.Log("CancelMove 완료");
                    
                    selectedUnit = null;
                    TurnManager.Instance.ShowUnitInfo(null); // 상태창 숨기기
                    
                    Debug.Log($"이동 완료: {selectedUnit?.name}이(가) {clickedTile.coordinates}로 이동");
                }
                else
                {
                    ShowGuideText("AP가 부족하여 이동할 수 없습니다.");
                }
            }
            else
            {
                Debug.Log("이동 범위를 벗어났습니다.");
            }
        }
        else
        {
            Debug.Log("이동할 수 없는 타일입니다 (유닛이 있음).");
        }
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
        if (TurnManager.Instance == null) return;
        
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 마우스 위치에서 Raycast로 타일이나 유닛 찾기
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            // 유닛 선택 처리
            Unit hitUnit = hit.collider.GetComponentInParent<Unit>();
            if (hitUnit != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("유닛 클릭됨: " + hitUnit.name);

                    // Case 1: The clicked unit is an enemy OR not the current player's unit.
                    if (hitUnit.playerId != TurnManager.Instance.currentPlayer)
                    {
                        // If we are in attack mode, and the clicked enemy is a valid target, attack it.
                        if (isAttacking && selectedUnit != null && selectedUnit.CanAttack(hitUnit))
                        {
                            HandleAttackClick(hitUnit.currentTile);
                        }
                        else
                        {
                            // Otherwise, just show the enemy's info and cancel any of our modes.
                            CancelAttack();
                            CancelMove();
                            SetNormalCursor();
                            selectedUnit = null;
                            TurnManager.Instance.ShowUnitInfo(hitUnit);
                        }
                    }
                    // Case 2: The clicked unit is a friendly unit for the current player.
                    else // This implies (hitUnit.playerId == TurnManager.Instance.currentPlayer)
                    {
                        // This is a selection or re-selection click.
                        // Cancel any previous action and start fresh with the newly clicked unit.
                        CancelAttack();
                        CancelMove();
                        SetNormalCursor();

                        selectedUnit = hitUnit;
                        TurnManager.Instance.ShowUnitInfo(hitUnit);

                        // Display move and attack options simultaneously on the grid.
                        if (!selectedUnit.hasMoved)
                        {
                            isMoving = true;
                            ShowMoveRange();
                        }
                        if (!selectedUnit.hasAttacked)
                        {
                            isAttacking = true;
                            ShowAttackRange();
                            // If there are targets, change the cursor to attack cursor.
                            if (attackRangeTiles.Count > 0)
                            {
                                SetAttackCursor();
                            }
                        }
                    }
                }
                return;
            }

            // 타일 처리
            HexTile tile = hit.collider.GetComponent<HexTile>();
            if (tile != null)
            {
               
                if (isPlacing)
                {
                    if (lastHighlightedTile != null && lastHighlightedTile != tile)
                        lastHighlightedTile.ResetHighlight();

                    tile.SetHighlight(new Color(1f, 0.7f, 0.2f));
                    lastHighlightedTile = tile;

                    if (Input.GetMouseButtonDown(0))
                    {
                        if (TurnManager.Instance.SpendAP(TurnManager.UNIT_PLACEMENT_COST))
                        {
                            PlaceUnit(tile);
                        }
                        else
                        {
                            ShowGuideText("AP가 부족하여 유닛을 배치할 수 없습니다.");
                        }
                        CancelPlacement(); // 안내문구 숨기고 UI 복구
                    }
                }
                else if (isAttacking && selectedUnit != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        HandleAttackClick(tile);
                    }
                    // 공격 가능한 적 유닛 위에 마우스 오버 시 하이라이트
                    if (tile.unitOnTile != null)
                    {
                        Unit targetUnit = tile.unitOnTile;
                        if (targetUnit != null && targetUnit.playerId != selectedUnit.playerId)
                        {
                            int distance = selectedUnit.GetDistanceToUnit(targetUnit);
                            if (distance <= selectedUnit.attackRange && distance > 0)
                            {
                                tile.SetHighlight(new Color(1f, 0.5f, 0.5f));
                            }
                        }
                    }
                }
                else if (isMoving && selectedUnit != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        HandleMoveClick(tile);
                    }
                }
                else if (isAttackMoving && selectedUnit != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        HandleAttackMoveClick(tile);
                    }
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    // 빈 타일을 클릭하면 UI 숨기기
                    TurnManager.Instance.ShowUnitInfo(null);
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            // 빈 공간을 클릭하면 UI 숨기기
            if (lastHighlightedTile != null)
            {
                lastHighlightedTile.ResetHighlight();
                lastHighlightedTile = null;
            }
            TurnManager.Instance.ShowUnitInfo(null);
        }

        // 우클릭으로 선택 취소
        if (Input.GetMouseButtonDown(1))
        {
            selectedUnit = null;
            CancelPlacement(); // 안내문구 숨기고 UI 복구
            CancelAttack();
            CancelMove();
            CancelAttackMove();
            SetNormalCursor(); // 커서 복원
            TurnManager.Instance.ShowUnitInfo(null);
        }
    }

    private void StartAttackMove()
    {
        Debug.Log("StartAttackMove 호출됨");
        if (selectedUnit == null || selectedUnit.hasMoved)
        {
            Debug.Log("이동할 수 있는 유닛이 없습니다.");
            return;
        }
        // 다른 모드 모두 취소
        CancelAttack();
        CancelMove();
        SetNormalCursor();
        isAttackMoving = true;
        ShowMoveRange();
        Debug.Log("공격 이동 모드 시작 완료");
    }

    private void CancelAttackMove()
    {
        Debug.Log("CancelAttackMove 호출됨");
        isAttackMoving = false;
        HideMoveRange();
        if (attackMoveButton != null)
        {
            attackMoveButton.gameObject.SetActive(false);
        }
        Debug.Log("CancelAttackMove 완료");
    }

    private void HandleAttackMoveClick(HexTile clickedTile)
    {
        if (selectedUnit == null || !isAttackMoving || clickedTile == null) return;

        Debug.Log("HandleAttackMoveClick 호출됨");

        if (clickedTile.unitOnTile == null)
        {
            int distance = selectedUnit.currentTile.GetDistanceTo(clickedTile);
            if (distance <= selectedUnit.moveRange && distance > 0)
            {
                if (TurnManager.Instance.SpendAP(TurnManager.ATTACK_MOVE_COST))
                {
                    Debug.Log($"공격 이동 시작: {selectedUnit.name} -> {clickedTile.coordinates}");
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
                Debug.Log("이동 범위를 벗어났습니다.");
            }
        }
        else
        {
            Debug.Log("이동할 수 없는 타일입니다 (유닛이 있음).");
        }
    }

    private void AttackMoveUnit(HexTile targetTile)
    {
        if (selectedUnit == null) { Debug.Log("selectedUnit is null"); return; }
        HexTile currentTile = hexGrid.GetTileAt(selectedUnit.currentTile.coordinates);
        targetTile = hexGrid.GetTileAt(targetTile.coordinates);
        if (currentTile == null) { Debug.Log("currentTile is null"); return; }
        if (targetTile == null) { Debug.Log("targetTile is null"); return; }
        if (targetTile.unitOnTile != null) { Debug.Log("targetTile already has a unit"); return; }
        var path = hexGrid.FindPath(currentTile, targetTile);
        if (path == null) { Debug.Log("No path found"); return; }
        if (path.Count < 2) { Debug.Log("Path too short"); return; }
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