using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;      // 카메라 이동 속도
    public float edgeSize = 10f;       // 화면 끝에서 몇 픽셀 이내면 이동할지
    public Vector2 minPosition = new Vector2(0, 0);   // 맵 최소 X,Z
    public Vector2 maxPosition = new Vector2(20, 15); // 맵 최대 X,Z (맵 크기에 맞게 조정)

    void Start()
    {
        // 시점 고정 (위에서 내려다보기)
        transform.rotation = Quaternion.Euler(90, 0, 0);
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

        // 맵 범위 제한
        pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
        pos.z = Mathf.Clamp(pos.z, minPosition.y, maxPosition.y);

        transform.position = pos;
    }
}