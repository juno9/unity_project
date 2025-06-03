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
    private HexTile lastHighlightedTile = null;
    private Unit selectedUnit = null;
    
    // 플레이어 관련 변수 추가
    private int currentPlayer = 1; // 1 또는 2
    private Color player1Color = Color.blue;
    private Color player2Color = new Color(1f, 0.5f, 0f); // 주황색
    public Button unitPlacementButton; // 버튼을 public으로 변경

    void Start()
    {
        Debug.Log("UnitPlacer Start 실행");
        
        // TurnManager가 없으면 생성
        if (TurnManager.Instance == null)
        {
            GameObject turnManagerObj = new GameObject("TurnManager");
            turnManagerObj.AddComponent<TurnManager>();
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
        rt.sizeDelta = new Vector2(150, 70);
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
                    TurnManager.Instance.ShowUnitInfo(hitUnit);
                    
                    if (TurnManager.Instance.IsUnitSelectable(hitUnit))
                    {
                        selectedUnit = hitUnit;
                    }
                }
                return;
            }

            // 타일 처리
            HexTile tile = hit.collider.GetComponent<HexTile>();
            if (tile != null)
            {
                Debug.Log($"클릭된 타일: {tile.name}, coordinates: {tile.coordinates}");
                if (isPlacing)
                {
                    if (lastHighlightedTile != null && lastHighlightedTile != tile)
                        lastHighlightedTile.ResetHighlight();

                    tile.SetHighlight(new Color(1f, 0.7f, 0.2f));
                    lastHighlightedTile = tile;

                    if (Input.GetMouseButtonDown(0))
                    {
                        PlaceUnit(tile);
                        CancelPlacement();
                    }
                }
                else if (selectedUnit != null && !selectedUnit.hasMoved)
                {
                    // 선택된 유닛 이동 처리
                    if (Input.GetMouseButtonDown(0))
                    {
                        MoveUnit(tile);
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
            CancelPlacement();
            TurnManager.Instance.ShowUnitInfo(null);
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
            tile.unitOnTile = newUnit;
            Debug.Log($"[배치] {unit.name}의 currentTile: {unit.currentTile != null}, tile: {tile.coordinates}");
            TurnManager.Instance.RegisterUnit(unit);
        }
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
        targetTile.unitOnTile = unit.gameObject;
        unit.hasMoved = true;
        selectedUnit = null;
        Debug.Log($"[이동] {unit.name}의 currentTile: {unit.currentTile != null}, tile: {targetTile.coordinates}");
    }

    public void StartPlacement()
    {
        Debug.Log($"플레이어 {TurnManager.Instance.currentPlayer} 턴 시작");
        isPlacing = true;
    }

    public void CancelPlacement()
    {
        isPlacing = false;
        if (lastHighlightedTile != null)
        {
            lastHighlightedTile.ResetHighlight();
            lastHighlightedTile = null;
        }
    }
} 