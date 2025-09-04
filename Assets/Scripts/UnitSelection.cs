using UnityEngine;
using System.Collections.Generic;

public class UnitSelection : MonoBehaviour
{
    private bool isDrawing = false;
    private Vector2 startPosition;
    private Texture2D rectangleTexture;

    private Unit[] allUnits; // Array to hold all units

    public void RefreshUnitList()
    {
        allUnits = FindObjectsOfType<Unit>();
        Debug.Log($"[UnitSelection] Unit list refreshed. Found {allUnits.Length} units.");
    }

    void Start()
    {
        // Create a 1x1 white texture for the rectangle
        rectangleTexture = new Texture2D(1, 1);
        rectangleTexture.SetPixel(0, 0, new Color(0.8f, 0.8f, 0.95f, 0.25f)); // A light blue, semi-transparent color
        rectangleTexture.Apply();

        // Find all units in the scene at the start
        allUnits = FindObjectsOfType<Unit>();
        Debug.Log($"[UnitSelection] Found {allUnits.Length} units.");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            startPosition = Input.mousePosition;

            // Deselect all units unless holding Shift (for additive selection)
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            {
                foreach (var unit in allUnits)
                {
                    if (unit.isSelected)
                    {
                        unit.SetSelected(false);
                    }
                }
            }
        }

        if (isDrawing)
        {
            // Update hover state for all units
            Rect selectionRect = GetScreenRect(startPosition, Input.mousePosition);
            foreach (var unit in allUnits)
            {
                Vector3 rawScreenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
                Vector2 screenPos = new Vector2(rawScreenPos.x, Screen.height - rawScreenPos.y); // Invert Y for GUI space

                // Set hover based on whether the unit is in the rect
                unit.SetHover(selectionRect.Contains(screenPos));
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
            Debug.Log("[UnitSelection] Mouse button up. Finalizing selection.");

            // Select all units within the final rectangle
            Rect selectionRect = GetScreenRect(startPosition, Input.mousePosition);
            int selectionCount = 0;
            bool loggedDebugInfo = false; // Add a flag to log only once per click

            foreach (var unit in allUnits)
            {
                // Ensure hover is turned off for all units after selection
                unit.SetHover(false);

                Vector3 rawScreenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
                Vector2 screenPos = new Vector2(rawScreenPos.x, Screen.height - rawScreenPos.y); // Invert Y for GUI space

                if (selectionRect.Contains(screenPos))
                {
                    unit.SetSelected(true);
                    selectionCount++;
                }
                else if (!loggedDebugInfo && unit != null) // If selection fails, log details for the first unit
                {
                    Debug.LogWarning($"--- UNIT NOT SELECTED DEBUG ---");
                    Debug.LogWarning($"Unit: {unit.name} at World Pos: {unit.transform.position}");
                    Debug.LogWarning($"Calculated Screen Pos (Y-top): {screenPos}");
                    Debug.LogWarning($"Selection Rect (Y-top): {selectionRect}");
                    Debug.LogWarning($"Is Rect containing Pos? {selectionRect.Contains(screenPos)}");
                    Debug.LogWarning($"---------------------------------");
                    loggedDebugInfo = true;
                }
            }
            Debug.Log($"[UnitSelection] Selected {selectionCount} units in this drag.");
        }
    }

    void OnGUI()
    {
        if (isDrawing)
        {
            Rect selectionRect = GetScreenRect(startPosition, Input.mousePosition);
            GUI.DrawTexture(selectionRect, rectangleTexture);
        }
    }

    // Helper function to create a Rect from two Vector2 points
    private Rect GetScreenRect(Vector2 screenPos1, Vector2 screenPos2)
    {
        // Move origin from bottom left to top left
        screenPos1.y = Screen.height - screenPos1.y;
        screenPos2.y = Screen.height - screenPos2.y;

        var topLeft = Vector2.Min(screenPos1, screenPos2);
        var bottomRight = Vector2.Max(screenPos1, screenPos2);

        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }
}