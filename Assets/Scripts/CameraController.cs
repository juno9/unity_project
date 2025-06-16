using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;      // 카메라 이동 속도
    public float edgeSize = 10f;       // 화면 끝에서 몇 픽셀 이내면 이동할지
    public Vector2 minPosition = new Vector2(0, -200);   // 맵 최소 X,Z (아래로 더 이동 가능)
    public Vector2 maxPosition = new Vector2(20, 25); // 맵 최대 X,Z (맵 크기에 맞게 조정)
    public Vector3 mapCenter;

    void Start()
    {
        float camHeight = 7f;
        float camDistance = 2f;
        // mapCenter는 HexGrid에서 할당해줌
        transform.position = new Vector3(mapCenter.x, camHeight, mapCenter.z - camDistance);
        transform.LookAt(new Vector3(mapCenter.x, 0, mapCenter.z));
        Camera cam = GetComponent<Camera>();
        if (cam != null) cam.fieldOfView = 70f;
    }

    void Update()
    {
        Vector3 pos = transform.position;
        Vector3 mousePos = Input.mousePosition;

        // 왼쪽
        if (mousePos.x <= edgeSize)
            pos.x -= moveSpeed * Time.deltaTime;
        // 오른쪽
        if (mousePos.x >= Screen.width - edgeSize)
            pos.x += moveSpeed * Time.deltaTime;
        // 아래
        if (mousePos.y <= edgeSize)
            pos.z -= moveSpeed * Time.deltaTime;
        // 위
        if (mousePos.y >= Screen.height - edgeSize)
            pos.z += moveSpeed * Time.deltaTime;

        // 맵 범위 제한 (z축: 화면 하단이 맵의 끝에 닿을 때까지)
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            float margin = 4.0f; // 아래로 보일 여유 공간(유닛)
            Vector3 bottomCenter = cam.ViewportToWorldPoint(new Vector3(0.5f, 0, cam.nearClipPlane + 1f));
            float bottomOffset = pos.z - bottomCenter.z;
            // pos.z를 minPosition.y + bottomOffset - margin까지 허용
            pos.z = Mathf.Clamp(pos.z, minPosition.y + bottomOffset - margin, maxPosition.y);
        }
        else
        {
            pos.z = Mathf.Clamp(pos.z, minPosition.y, maxPosition.y);
        }
        pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);

        transform.position = pos;
    }
}