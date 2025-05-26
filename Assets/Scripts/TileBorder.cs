using UnityEngine;

public class TileBorder : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public float lineWidth = 0.05f;
    public Color lineColor = Color.black;
    public float radius = 0.5f; // 육각형 반지름 (HexTile의 크기에 맞게 조정)

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        int points = 6;
        Vector3[] positions = new Vector3[points];
        for (int i = 0; i < points; i++)
        {
            float angle = Mathf.Deg2Rad * (60 * i);
            positions[i] = new Vector3(Mathf.Cos(angle) * radius, 0.01f, Mathf.Sin(angle) * radius);
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }
} 