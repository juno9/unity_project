using UnityEngine;
using System.Collections.Generic;

public class HexTile : MonoBehaviour
{
    public Vector2Int coordinates; // Grid coordinates
    public Vector3 position;      // World position
    public Unit unitOnTile;       // 이 타일에 있는 유닛
    public List<HexTile> neighbors = new List<HexTile>(); // 경로 탐색용 이웃 타일
    
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

    public void PlaceUnit(GameObject unitPrefab)
    {
        if (unitOnTile == null && unitPrefab != null)
        {
            try
            {
                // 유닛 생성 및 위치 설정
                GameObject unitObject = Instantiate(unitPrefab, transform.position + Vector3.up * 0.6f, Quaternion.identity);
                
                // 부모 설정
                unitObject.transform.SetParent(transform);
                
                // 이름 설정
                unitObject.name = "Unit_" + coordinates.x + "_" + coordinates.y;
                
                // Unit 컴포넌트 가져오기
                Unit unit = unitObject.GetComponent<Unit>();
                if (unit == null)
                {
                    unit = unitObject.AddComponent<Unit>();
                }
                
                // 타일과 유닛 연결
                unitOnTile = unit;
                unit.currentTile = this;
                
                // 활성화
                unitObject.SetActive(true);
                
                Debug.Log($"Unit placed at tile ({coordinates.x}, {coordinates.y})");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error placing unit: {e.Message}");
                if (unitOnTile != null)
                {
                    Destroy(unitOnTile.gameObject);
                    unitOnTile = null;
                }
            }
        }
    }
} 