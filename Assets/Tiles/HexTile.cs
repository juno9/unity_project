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
    private bool isOccupied = false; // 타일 점유 상태

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
            Debug.Log($"[HexTile] {coordinates} 색상 변경: {color}");
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

    // 타일 점유 상태 확인
    public bool IsOccupied()
    {
        return isOccupied || unitOnTile != null;
    }

    // 타일 점유 상태 설정
    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
    }

    // 두 타일 간의 거리 계산 (육각형 그리드)
    public int GetDistanceTo(HexTile other)
    {
        if (other == null) return int.MaxValue;

        // 현재 타일(self)의 큐브 좌표 변환
        int self_q = coordinates.x - (coordinates.y + (coordinates.y & 1)) / 2;
        int self_r = coordinates.y;
        int self_s = -self_q - self_r;

        // 다른 타일(other)의 큐브 좌표 변환
        int other_q = other.coordinates.x - (other.coordinates.y + (other.coordinates.y & 1)) / 2;
        int other_r = other.coordinates.y;
        int other_s = -other_q - other_r;

        // 큐브 좌표계에서의 거리 계산
        return (Mathf.Abs(self_q - other_q) 
              + Mathf.Abs(self_r - other_r) 
              + Mathf.Abs(self_s - other_s)) / 2;
    }

    public void PlaceUnit(GameObject unitObject)
    {
        if (unitOnTile == null && unitObject != null)
        {
            // 위치만 맞추고, 회전은 건드리지 않음
            unitObject.transform.position = transform.position + Vector3.up * 0.6f;
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
            isOccupied = true;
            unitObject.SetActive(true);
            Debug.Log($"Unit placed at tile ({coordinates.x}, {coordinates.y})");
        }
    }
} 