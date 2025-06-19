using UnityEngine;

public class HexTile : MonoBehaviour
{
    public Vector2Int coordinates; // Grid coordinates
    public Vector3 position;      // World position
    public Unit unitOnTile;       // 이 타일에 있는 유닛
    
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

    public void SetHighlight(Color color)
    {
        SetColor(color);
    }

    public void ResetHighlight()
    {
        SetColor(Color.white);
    }

    // 두 타일 간의 거리 계산 (육각형 그리드)
    public int GetDistanceTo(HexTile other)
    {
        if (other == null) return int.MaxValue;
        
        Vector2Int delta = coordinates - other.coordinates;
        
        // 육각형 그리드에서의 거리 계산
        int distance = Mathf.Max(
            Mathf.Abs(delta.x),
            Mathf.Abs(delta.y),
            Mathf.Abs(delta.x + delta.y)
        );
        
        return distance;
    }
} 