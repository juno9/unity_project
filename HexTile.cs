using UnityEngine;
using System.Collections.Generic;

public class HexTile : MonoBehaviour
{
    public Vector3Int coordinates; // Cube coordinates
    public Vector3 position;      // World position
    public Unit unitOnTile;       // 이 타일에 있는 유닛
    public List<HexTile> neighbors = new List<HexTile>();

    public bool isBase = false; // 거점 여부

    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Initialize(Vector2Int gridPosition)
    {
        coordinates = new Vector3Int(gridPosition.x, gridPosition.y, -gridPosition.x - gridPosition.y);
    }

    public void FindNeighbors(HexGrid hexGrid)
    {
        neighbors.Clear();
        Vector3Int[] neighborCoords = new Vector3Int[]
        {
            new Vector3Int(1, 0, -1), new Vector3Int(-1, 0, 1),
            new Vector3Int(0, 1, -1), new Vector3Int(0, -1, 1),
            new Vector3Int(1, -1, 0), new Vector3Int(-1, 1, 0)
        };

        foreach (var coord in neighborCoords)
        {
            HexTile neighbor = hexGrid.GetTileAt(coordinates + coord);
            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }
    }

    public void SetBase()
    {
        isBase = true;
        SetColor(Color.green);
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

    // 두 타일 간의 거리 계산 (큐브 좌표계)
    public int GetDistanceTo(HexTile other)
    {
        if (other == null) return int.MaxValue;
        return (Mathf.Abs(coordinates.x - other.coordinates.x) +
                Mathf.Abs(coordinates.y - other.coordinates.y) +
                Mathf.Abs(coordinates.z - other.coordinates.z)) / 2;
    }

    // 추가된 메서드
    public bool IsOccupied()
    {
        return unitOnTile != null;
    }

    public void SetOccupied(bool isOccupied)
    {
        // 이 메서드는 Unit.cs에서 직접 unitOnTile을 설정/해제하므로
        // 여기서는 특별한 로직이 필요 없을 수 있습니다.
        // 필요에 따라 점유 상태 변경 시 시각적 피드백 등을 추가할 수 있습니다.
    }

    public void PlaceUnit(GameObject unitObject)
    {
        if (unitObject != null)
        {
            unitOnTile = unitObject.GetComponent<Unit>();
            if (unitOnTile != null)
            {
                unitOnTile.currentTile = this;
                unitObject.transform.position = transform.position + new Vector3(0, 0.5f, 0); // 유닛의 높이를 타일 위에 맞춤
            }
        }
    }

    public void RemoveUnit()
    {
        if (unitOnTile != null)
        {
            unitOnTile.currentTile = null;
            unitOnTile = null;
        }
    }
} 