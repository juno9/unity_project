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

        MeshFilter mf = GetComponent<MeshFilter>();
        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc == null)
            mc = gameObject.AddComponent<MeshCollider>();
        if (mf != null && mc != null)
            mc.sharedMesh = mf.sharedMesh;
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
        if (unitOnTile == null && unitPrefab != null)
        {
            try
            {
                // 유닛 생성 및 위치 설정
                unitOnTile = Instantiate(unitPrefab, transform.position + Vector3.up * 0.6f, Quaternion.identity);
                
                // 부모 설정
                unitOnTile.transform.SetParent(transform);
                
                // 이름 설정
                unitOnTile.name = "Unit_" + coordinates.x + "_" + coordinates.y;
                
                // 활성화
                unitOnTile.SetActive(true);
                
                Debug.Log($"Unit placed at tile ({coordinates.x}, {coordinates.y})");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error placing unit: {e.Message}");
                if (unitOnTile != null)
                {
                    Destroy(unitOnTile);
                    unitOnTile = null;
                }
            }
        }
    }
} 