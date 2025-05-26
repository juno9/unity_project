using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridSizeUI : MonoBehaviour
{
    public HexGrid hexGrid;
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public Button applyButton;

    void Start()
    {
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(ApplySize);
        }

        // Set initial values
        if (hexGrid != null)
        {
            if (widthInput != null) widthInput.text = hexGrid.mapWidth.ToString();
            if (heightInput != null) heightInput.text = hexGrid.mapHeight.ToString();
        }
    }

    public void ApplySize()
    {
        if (hexGrid == null) return;

        int width = 10;
        int height = 10;

        if (int.TryParse(widthInput.text, out int w))
        {
            width = Mathf.Clamp(w, 1, 100);
        }

        if (int.TryParse(heightInput.text, out int h))
        {
            height = Mathf.Clamp(h, 1, 100);
        }

        hexGrid.mapWidth = width;
        hexGrid.mapHeight = height;
        hexGrid.GenerateGrid();
    }
}
