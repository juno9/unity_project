using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitSelection : MonoBehaviour
{
    private UnitPlacer unitPlacer; // UnitPlacer 참조

    private bool isDrawing = false;
    private Vector2 startPosition;
    private float clickStartTime;
    private Texture2D rectangleTexture;

    private const float MAX_CLICK_DURATION = 0.2f;
    private const float MAX_CLICK_DISTANCE = 5f;

    private Unit[] allUnits;

    public void RefreshUnitList()
    {
        allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
    }

    void Start()
    {
        // 씬에 있는 UnitPlacer의 단일 인스턴스를 찾습니다.
        unitPlacer = FindFirstObjectByType<UnitPlacer>();
        if (unitPlacer == null)
        {
            Debug.LogError("[UnitSelection] 씬에서 UnitPlacer를 찾을 수 없습니다!");
        }

        rectangleTexture = new Texture2D(1, 1);
        rectangleTexture.SetPixel(0, 0, new Color(0.8f, 0.8f, 0.95f, 0.25f));
        rectangleTexture.Apply();

        allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
    }

    void Update()
    {
        // UI 요소 위에 마우스가 있으면 선택 로직을 실행하지 않습니다.
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        if (unitPlacer == null) return; // UnitPlacer가 없으면 아무것도 하지 않습니다.

        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            startPosition = Input.mousePosition;
            clickStartTime = Time.time;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!isDrawing) return; // 이미 처리가 끝났으면 중복 실행 방지
            isDrawing = false;

            float clickDuration = Time.time - clickStartTime;
            float dragDistance = Vector2.Distance(startPosition, Input.mousePosition);

            if (clickDuration < MAX_CLICK_DURATION && dragDistance < MAX_CLICK_DISTANCE)
            {
                HandleSingleClick();
            }
            else
            {
                HandleDragSelection();
            }
        }
    }

    private void HandleSingleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        { 
            Unit clickedUnit = hit.collider.GetComponentInParent<Unit>();
            if (clickedUnit != null)
            {
                unitPlacer.SelectUnit(new List<Unit> { clickedUnit });
            }
            else
            {
                unitPlacer.SelectUnit(new List<Unit>());
            }
        }
        else
        {
            // 씬의 빈 공간을 클릭하면 모두 선택 해제합니다.
            unitPlacer.SelectUnit(new List<Unit>());
        }
    }

    private void HandleDragSelection()
    {
        Rect selectionRect = GetScreenRect(startPosition, Input.mousePosition);
        
        List<Unit> newlySelectedUnits = new List<Unit>();
        
        foreach (var unit in allUnits)
        {
            if (unit == null) continue;
            Vector3 rawScreenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
            Vector2 screenPos = new Vector2(rawScreenPos.x, Screen.height - rawScreenPos.y);

            if (selectionRect.Contains(screenPos))
            {
                newlySelectedUnits.Add(unit);
            }
        }
        
        unitPlacer.SelectUnit(newlySelectedUnits);
    }

    void OnGUI()
    {
        if (isDrawing)
        {
            float dragDistance = Vector2.Distance(startPosition, Input.mousePosition);
            if (dragDistance > MAX_CLICK_DISTANCE)
            {
                Rect selectionRect = GetScreenRect(startPosition, Input.mousePosition);
                GUI.DrawTexture(selectionRect, rectangleTexture);
            }
        }
    }

    private Rect GetScreenRect(Vector2 screenPos1, Vector2 screenPos2)
    {
        screenPos1.y = Screen.height - screenPos1.y;
        screenPos2.y = Screen.height - screenPos2.y;
        var topLeft = Vector2.Min(screenPos1, screenPos2);
        var bottomRight = Vector2.Max(screenPos1, screenPos2);
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }
}
