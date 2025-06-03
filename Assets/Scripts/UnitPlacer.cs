using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitPlacer : MonoBehaviour
{
    public GameObject unitPrefab;
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

        if (unitPrefab == null)
        {
            // 프리팹 생성
            GameObject cubePrefab = new GameObject("UnitCubePrefab");
            
            // 필요한 컴포넌트 추가
            MeshFilter mf = cubePrefab.AddComponent<MeshFilter>();
            MeshRenderer mr = cubePrefab.AddComponent<MeshRenderer>();
            Unit unit = cubePrefab.AddComponent<Unit>();
            BoxCollider collider = cubePrefab.AddComponent<BoxCollider>();
            collider.size = Vector3.one * 0.7f; // 유닛 크기에 맞게 조정
            
            // 큐브 메시 생성
            Mesh cubeMesh = new Mesh();
            Vector3[] vertices = new Vector3[8];
            vertices[0] = new Vector3(-0.5f, -0.5f, -0.5f);
            vertices[1] = new Vector3(0.5f, -0.5f, -0.5f);
            vertices[2] = new Vector3(0.5f, 0.5f, -0.5f);
            vertices[3] = new Vector3(-0.5f, 0.5f, -0.5f);
            vertices[4] = new Vector3(-0.5f, -0.5f, 0.5f);
            vertices[5] = new Vector3(0.5f, -0.5f, 0.5f);
            vertices[6] = new Vector3(0.5f, 0.5f, 0.5f);
            vertices[7] = new Vector3(-0.5f, 0.5f, 0.5f);

            int[] triangles = new int[36];
            // 앞면
            triangles[0] = 0; triangles[1] = 2; triangles[2] = 1;
            triangles[3] = 0; triangles[4] = 3; triangles[5] = 2;
            // 뒷면
            triangles[6] = 5; triangles[7] = 7; triangles[8] = 4;
            triangles[9] = 5; triangles[10] = 6; triangles[11] = 7;
            // 윗면
            triangles[12] = 3; triangles[13] = 7; triangles[14] = 6;
            triangles[15] = 3; triangles[16] = 6; triangles[17] = 2;
            // 아랫면
            triangles[18] = 1; triangles[19] = 5; triangles[20] = 4;
            triangles[21] = 1; triangles[22] = 4; triangles[23] = 0;
            // 왼쪽면
            triangles[24] = 0; triangles[25] = 4; triangles[26] = 7;
            triangles[27] = 0; triangles[28] = 7; triangles[29] = 3;
            // 오른쪽면
            triangles[30] = 1; triangles[31] = 2; triangles[32] = 6;
            triangles[33] = 1; triangles[34] = 6; triangles[35] = 5;

            cubeMesh.vertices = vertices;
            cubeMesh.triangles = triangles;
            cubeMesh.RecalculateNormals();
            cubeMesh.RecalculateBounds();

            mf.mesh = cubeMesh;

            // 머티리얼 설정
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            if (shader != null)
            {
                mr.material = new Material(shader);
                mr.material.color = player1Color;
            }
            
            // 크기 설정
            cubePrefab.transform.localScale = Vector3.one * 0.7f;
            
            unitPrefab = cubePrefab;
            unitPrefab.SetActive(false);
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
            Unit hitUnit = hit.collider.GetComponent<Unit>();
            if (hitUnit != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    // 유닛 정보 표시
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
        if (tile.unitOnTile == null)
        {
            GameObject newUnit = Instantiate(unitPrefab, tile.transform.position + Vector3.up * 0.6f, Quaternion.identity);
            newUnit.transform.SetParent(tile.transform);
            newUnit.name = $"Unit_{tile.coordinates.x}_{tile.coordinates.y}";
            
            Unit unit = newUnit.GetComponent<Unit>();
            unit.playerId = TurnManager.Instance.currentPlayer;
            
            MeshRenderer mr = newUnit.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material.color = unit.playerId == 1 ? player1Color : player2Color;
            }

            // Collider 추가
            BoxCollider collider = newUnit.GetComponent<BoxCollider>();
            collider.size = Vector3.one * 0.7f; // 유닛 크기에 맞게 조정
            
            newUnit.SetActive(true);
            tile.unitOnTile = newUnit;
            
            TurnManager.Instance.RegisterUnit(unit);
        }
    }

    private void MoveUnit(HexTile targetTile)
    {
        if (targetTile.unitOnTile == null)
        {
            HexTile currentTile = selectedUnit.GetComponentInParent<HexTile>();
            if (currentTile != null)
            {
                currentTile.unitOnTile = null;
                selectedUnit.transform.SetParent(targetTile.transform);
                selectedUnit.transform.position = targetTile.transform.position + Vector3.up * 0.6f;
                targetTile.unitOnTile = selectedUnit.gameObject;
                selectedUnit.hasMoved = true;
                selectedUnit = null;
            }
        }
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