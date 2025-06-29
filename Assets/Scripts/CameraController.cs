using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("카메라 이동 설정")]
    public float moveSpeed = 10f;      // 카메라 이동 속도
    public float edgeSize = 10f;       // 화면 끝에서 몇 픽셀 이내면 이동할지
    public Vector2 minPosition = new Vector2(0, -200);   // 맵 최소 X,Z (아래로 더 이동 가능)
    public Vector2 maxPosition = new Vector2(20, 25); // 맵 최대 X,Z (맵 크기에 맞게 조정)
    public Vector3 mapCenter;
    
    [Header("턴 전환 카메라 설정")]
    public bool enableTurnTransition = true; // 턴 전환 시 카메라 회전 활성화
    public float turnTransitionHeight = 10f; // 턴 전환 시 카메라 높이
    public float turnTransitionDistance = 5f; // 턴 전환 시 카메라 거리
    
    private Vector3 originalPosition; // 원래 카메라 위치
    private Quaternion originalRotation; // 원래 카메라 회전
    private int currentPlayerView = 1; // 현재 카메라가 바라보는 플레이어
    private bool isRegistered = false; // 등록 상태 추적
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float initialOrthoSize;

    void Start()
    {
        // mapCenter 자동 설정
        if (mapCenter == Vector3.zero)
        {
            HexGrid hexGrid = FindFirstObjectByType<HexGrid>();
            if (hexGrid != null)
            {
                mapCenter = hexGrid.GetMapCenter();
                Debug.Log($"mapCenter가 자동으로 설정되었습니다: {mapCenter}");
            }
            else
            {
                // HexGrid가 없으면 기본값 설정
                mapCenter = new Vector3(10f, 0f, 7.5f); // 20x15 맵의 중앙
                Debug.LogWarning("HexGrid를 찾을 수 없어 기본 mapCenter를 사용합니다.");
            }
        }
        
        // 최초 카메라 위치/회전/orthographicSize 저장
        Camera cam = GetComponent<Camera>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        if (cam != null)
        {
            cam.orthographic = true;
            initialOrthoSize = cam.orthographicSize;
        }
        else
        {
            initialOrthoSize = 12f;
        }
        
        originalPosition = initialPosition;
        originalRotation = initialRotation;
        
        // TurnManager 등록 시도
        TryRegisterWithTurnManager();
    }

    void Update()
    {
        // TurnManager 등록 재시도 (매 프레임마다 시도)
        if (TurnManager.Instance != null && !isRegistered)
        {
            TryRegisterWithTurnManager();
        }
        
        Vector3 pos = transform.position;
        Vector3 mousePos = Input.mousePosition;

        // 현재 플레이어에 따라 이동 방향 결정
        int moveDirection = 1;
        bool isPlayer2 = false;
        if (TurnManager.Instance != null && TurnManager.Instance.currentPlayer == 2)
        {
            moveDirection = -1; // 플레이어 2일 때는 반대 방향
            isPlayer2 = true;
        }

        // 왼쪽
        if (mousePos.x <= edgeSize)
            pos.x -= moveSpeed * Time.deltaTime * moveDirection;
        // 오른쪽
        if (mousePos.x >= Screen.width - edgeSize)
            pos.x += moveSpeed * Time.deltaTime * moveDirection;
        // 아래
        if (mousePos.y <= edgeSize)
            pos.z -= moveSpeed * Time.deltaTime * moveDirection;
        // 위
        if (mousePos.y >= Screen.height - edgeSize)
            pos.z += moveSpeed * Time.deltaTime * moveDirection;

        // 맵 범위 제한 (z축: 화면 하단이 맵의 끝에 닿을 때까지)
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            float margin = 4.0f; // 아래로 보일 여유 공간(유닛)
            Vector3 bottomCenter = cam.ViewportToWorldPoint(new Vector3(0.5f, 0, cam.nearClipPlane + 1f));
            float bottomOffset = pos.z - bottomCenter.z;
            if (!isPlayer2)
            {
                // 플레이어 1: 기존 방식
                pos.z = Mathf.Clamp(pos.z, minPosition.y, maxPosition.y);
            }
            else
            {
                // 플레이어 2: Clamp 기준 반대로 적용
                float minZ = minPosition.y;
                float maxZ = maxPosition.y;
                // 아래쪽 끝까지 이동 가능하도록 Clamp를 반대로 적용
                pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
            }
        }
        else
        {
            pos.z = Mathf.Clamp(pos.z, minPosition.y, maxPosition.y);
        }
        pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);

        transform.position = pos;
    }
    
    private void TryRegisterWithTurnManager()
    {
        if (isRegistered) return; // 이미 등록되어 있으면 스킵
        
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.RegisterCameraController(this);
            isRegistered = true;
            Debug.Log("CameraController가 TurnManager에 등록되었습니다.");
        }
    }
    
    // 턴 전환 시 카메라 회전
    public void TransitionToPlayerView(int playerId)
    {
        Debug.Log($"TransitionToPlayerView 호출됨: playerId={playerId}, enableTurnTransition={enableTurnTransition}, currentPlayerView={currentPlayerView}");
        
        if (!enableTurnTransition || currentPlayerView == playerId) 
        {
            Debug.Log($"카메라 전환이 취소됨: enableTurnTransition={enableTurnTransition}, currentPlayerView={currentPlayerView}");
            return;
        }
        
        Debug.Log($"카메라 전환 시작: 플레이어 {playerId} 시점으로");
        TransitionCameraImmediate(playerId);
    }
    
    private void TransitionCameraImmediate(int targetPlayerId)
    {
        currentPlayerView = targetPlayerId;

        // 최초 카메라 위치/회전/orthographicSize 기준으로 대칭 변환
        Vector3 basePos = initialPosition;
        Quaternion baseRot = initialRotation;
        float orthoSize = initialOrthoSize;
        Camera cam = GetComponent<Camera>();

        Vector3 targetPosition = basePos;
        Quaternion targetRotation = baseRot;

        if (targetPlayerId == 1)
        {
            // 플레이어 1: 최초 시점 그대로
            targetPosition = basePos;
            targetRotation = baseRot;
        }
        else
        {
            // 플레이어 2: z축 대칭, 회전도 z축 기준 반전
            Vector3 offset = basePos - mapCenter;
            offset.z = -offset.z;
            targetPosition = mapCenter + offset;

            // 회전도 z축 기준 반전 (y축만 180도 회전)
            targetRotation = Quaternion.Euler(baseRot.eulerAngles.x, baseRot.eulerAngles.y + 180f, baseRot.eulerAngles.z);
        }

        // 즉시 위치, 회전, Orthographic 모드/사이즈 변경
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = orthoSize;
        }

        Debug.Log($"카메라가 플레이어 {targetPlayerId} 시점으로 직교로 즉시 전환되었습니다.");
    }
    
    // 카메라를 원래 위치로 리셋
    public void ResetCamera()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        currentPlayerView = 1;
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = initialOrthoSize;
        }
        Debug.Log("카메라가 원래 위치로 직교로 즉시 리셋되었습니다.");
    }
}