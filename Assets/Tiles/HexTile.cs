using UnityEngine;

public class HexTile : MonoBehaviour
{
    public Vector2Int coordinates; // Grid coordinates
    public Vector3 position;      // World position
    public GameObject unitOnTile;
    
    private MeshRenderer meshRenderer;
    private Color originalColor;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            originalColor = meshRenderer.material.color;
    }

    public void Initialize(Vector2Int coords)
    {
        coordinates = coords;
    }

    public void SetColor(Color color)
    {
        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material.color = color;
        }
    }

    public void SetHighlight(Color color)
    {
        if (meshRenderer != null)
            meshRenderer.material.color = color;
    }

    public void ResetHighlight()
    {
        if (meshRenderer != null)
            meshRenderer.material.color = originalColor;
    }

    public void PlaceUnit(GameObject unitPrefab)
    {
        if (unitOnTile == null)
        {
            unitOnTile = GameObject.Instantiate(unitPrefab, transform.position + Vector3.up * 0.6f, Quaternion.identity);
            unitOnTile.SetActive(true);
        }
    }
} 