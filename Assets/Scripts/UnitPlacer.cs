using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitPlacer : MonoBehaviour
{
    public GameObject unitPrefab;
    public HexGrid hexGrid;
    private bool isPlacing = false;
    private HexTile lastHighlightedTile = null;

    void Start()
    {
        if (unitPrefab == null)
        {
            GameObject cubePrefab = new GameObject("UnitCubePrefab");
            cubePrefab.AddComponent<UnitCubePrefab>();
            unitPrefab = cubePrefab;
            unitPrefab.SetActive(false); // 씬에 보이지 않게
        }
        if (hexGrid == null)
        {
            hexGrid = FindObjectOfType<HexGrid>();
        }
        // 유닛 배치 버튼 동적 생성 및 연결
        CreateUnitPlacementButton();
    }

    private void CreateUnitPlacementButton()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        GameObject buttonObj = new GameObject("UnitPlaceButton");
        buttonObj.transform.SetParent(canvas.transform);
        Button button = buttonObj.AddComponent<Button>();
        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 1f, 1f);
        RectTransform rt = buttonObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120, 40);
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -20); // 우측 상단에서 약간 안쪽
        // 텍스트 추가
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        Text text = textObj.AddComponent<Text>();
        text.text = "유닛 배치";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        // 버튼 클릭 이벤트 연결
        button.onClick.AddListener(StartPlacement);
    }

    void Update()
    {
        if (!isPlacing) return;

        // 마우스 우클릭 시 배치 취소
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
            return;
        }

        // UI 위에 있으면 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 마우스 위치에서 Raycast로 타일 찾기
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            HexTile tile = hit.collider.GetComponent<HexTile>();
            if (tile != null)
            {
                if (lastHighlightedTile != null && lastHighlightedTile != tile)
                    lastHighlightedTile.ResetHighlight();

                tile.SetHighlight(Color.red);
                lastHighlightedTile = tile;

                // 좌클릭 시 유닛 배치
                if (Input.GetMouseButtonDown(0))
                {
                    tile.PlaceUnit(unitPrefab);
                    CancelPlacement();
                }
            }
        }
        else if (lastHighlightedTile != null)
        {
            lastHighlightedTile.ResetHighlight();
            lastHighlightedTile = null;
        }
    }

    public void StartPlacement()
    {
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