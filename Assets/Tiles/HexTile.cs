using UnityEngine;

public class HexTile : MonoBehaviour
{
    public Vector2Int coordinates; // Grid coordinates
    public Vector3 position;      // World position
    
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
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
} 