using UnityEngine;

public class TileBorder : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public float lineWidth = 0.05f;
    public Color lineColor = Color.black;
    public float radius = 0.5f; // 육각형 반지름 (HexTile의 크기에 맞게 조정)

    void Awake()
    {
        InitializeLineRenderer();
    }

    private void InitializeLineRenderer()
    {
        // LineRenderer가 이미 있는지 확인하고, 없으면 추가합니다.
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Shader defaultShader = Shader.Find("Sprites/Default");
        if (defaultShader == null)
        {
            Debug.LogError("TileBorder: 'Sprites/Default' 셰이더를 찾을 수 없습니다. 기본 머티리얼을 사용할 수 없습니다.");
            // 대체 셰이더나 머티리얼을 사용하거나, 오류 처리
            lineRenderer.material = new Material(Shader.Find("Standard")); // 대체 셰이더
        }
        else
        {
            lineRenderer.material = new Material(defaultShader);
        }

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

    public void SetBorderColor(Color color)
    {
        if (lineRenderer == null)
        {
            Debug.LogError("TileBorder: LineRenderer가 초기화되지 않았습니다. 색상 변경 불가.");
            return;
        }
        lineColor = color;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
    }
}