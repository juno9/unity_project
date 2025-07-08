using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        // 플레이어에 상관없이 항상 같은 시야로 설정
        Vector3 basePos = initialPosition;
        Quaternion baseRot = initialRotation;
        float orthoSize = initialOrthoSize;
        Camera cam = GetComponent<Camera>();

        Vector3 targetPosition = basePos;
        Quaternion targetRotation = baseRot;
        minPosition = new Vector2(0, -200);
        maxPosition = new Vector2(20, 25);

        // 즉시 위치, 회전, Orthographic 모드/사이즈 변경
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = orthoSize;
        }

        Debug.Log($"카메라가 플레이어 {targetPlayerId} 시점으로(공통 시야) 즉시 전환되었습니다.");
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