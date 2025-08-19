using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [Header("카메라 이동/경계 설정")]
    public float moveSpeed = 20f;
    public float edgeSize = 20f;
    public Vector2 minPosition = new Vector2(-10, -10);
    public Vector2 maxPosition = new Vector2(30, 30);

    [Header("카메라 줌 설정")]
    public float zoomSpeed = 5f;
    public float minOrthoSize = 5f;
    public float maxOrthoSize = 20f;

    [Header("턴 전환/시점 설정")]
    public Vector3 mapCenter = Vector3.zero;
    public float cameraHeight = 15f;
    public float cameraDistance = 10f;

    private Base player1Base;
    private Base player2Base;
    private Camera cam;
    private bool isRegistered = false;
    private int currentPlayerView = 1; // 이 변수가 HandleKeyboardPan에서 사용됩니다.

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true; // 항상 직교 모드로 설정

        // 맵 중앙 자동 설정
        if (mapCenter == Vector3.zero)
        {
            HexGrid hexGrid = FindFirstObjectByType<HexGrid>();
            if (hexGrid != null) mapCenter = hexGrid.GetMapCenter();
            else mapCenter = new Vector3(10, 0, 7.5f);
        }

        // 각 플레이어의 Base 찾기
        Base[] allBases = FindObjectsByType<Base>(FindObjectsSortMode.None);
        Debug.Log($"[Camera Log] CameraController found {allBases.Length} Base objects in scene at Start().");
        foreach (Base b in allBases)
        {
            if (b.playerId == 1) player1Base = b;
            else if (b.playerId == 2) player2Base = b;
        }
        if (player1Base == null) Debug.LogWarning("[Camera Log] Player 1 Base not found at Start().");
        if (player2Base == null) Debug.LogWarning("[Camera Log] Player 2 Base not found at Start().");

        // TurnManager 등록 시도
        TryRegisterWithTurnManager();
        
        // 초기 카메라 위치 설정은 GameInitializer에서 호출하도록 변경
        // TransitionToPlayerView(1);
    }

    void Update()
    {
        if (TurnManager.Instance != null && !isRegistered) TryRegisterWithTurnManager();

        HandleMouseZoom();
        HandleKeyboardPan();
    }

    private void HandleMouseZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minOrthoSize, maxOrthoSize);
        }
    }

    private void HandleKeyboardPan()
    {
        Vector3 pos = transform.position;
        Vector3 initialPos = pos; // 로그용 초기 위치

        float horizontalInput = Input.GetAxis("Horizontal"); // A/D keys
        float verticalInput = Input.GetAxis("Vertical");   // W/S keys

        // 플레이어 2 턴일 때 수직 입력 반전 (월드 Z축 기준)
        if (cam != null && cam.orthographic && currentPlayerView == 2)
        {
            verticalInput *= -1; // Player 2 턴일 때만 Z축 이동 방향 반전
            Debug.Log($"[Camera Log] Player 2 turn: Vertical input inverted for world Z-axis to {verticalInput}");
        }
        else
        {
            Debug.Log($"[Camera Log] Player 1 turn: Vertical input for world Z-axis {verticalInput}");
        }

        // 월드 X축 이동
        pos.x += horizontalInput * moveSpeed * Time.deltaTime;
        // 월드 Z축 이동
        pos.z += verticalInput * moveSpeed * Time.deltaTime;

        Debug.Log($"[Camera Log] Raw Input: H={horizontalInput}, V={Input.GetAxis("Vertical")}");
        Debug.Log($"[Camera Log] Adjusted V Input (for world Z): {verticalInput}");
        Debug.Log($"[Camera Log] Pos before clamp: {pos}");

        pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
        pos.z = Mathf.Clamp(pos.z, minPosition.y, maxPosition.y); // minPosition.y/maxPosition.y는 Z축 경계

        Debug.Log($"[Camera Log] Pos after clamp: {pos}");
        Debug.Log($"[Camera Log] Movement Delta: {pos - initialPos}");

        transform.position = pos;
    }

    private void TryRegisterWithTurnManager()
    {
        if (isRegistered) return;
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.RegisterCameraController(this);
            isRegistered = true;
        }
    }

    public void TransitionToPlayerView(int playerId)
    {
        // Base가 Start()에서 찾아지지 않았을 경우, 다시 찾아보기
        if (player1Base == null || player2Base == null)
        {
            Base[] allBases = FindObjectsByType<Base>(FindObjectsSortMode.None);
            foreach (Base b in allBases)
            {
                if (b.playerId == 1) player1Base = b;
                else if (b.playerId == 2) player2Base = b;
            }
            if (player1Base == null) Debug.LogWarning("[Camera Log] Player 1 Base still not found during Transition retry.");
            if (player2Base == null) Debug.LogWarning("[Camera Log] Player 2 Base still not found during Transition retry.");
        }

        Base targetBase = (playerId == 1) ? player1Base : player2Base;
        Base opponentBase = (playerId == 1) ? player2Base : player1Base; // 상대방 Base

        if (targetBase == null || opponentBase == null)
        {
            Debug.LogWarning($"[Camera Log] 플레이어 {playerId}의 Base 또는 상대방 Base를 찾을 수 없어 카메라를 이동할 수 없습니다. (Base 객체가 존재하지 않음)");
            // Base가 없을 경우 맵 중앙을 기준으로 기본 위치 설정
            transform.position = new Vector3(mapCenter.x, cameraHeight, mapCenter.z - cameraDistance);
            transform.rotation = Quaternion.Euler(45, 0, 0); // 기본 45도 각도
            return;
        }

        // 목표 위치 계산 (Base 위치에서 상대방 Base 반대 방향으로 물러나기 + 높이)
        Vector3 directionAwayFromOpponent = (targetBase.transform.position - opponentBase.transform.position).normalized;
        Vector3 targetPosition = targetBase.transform.position + directionAwayFromOpponent * cameraDistance + Vector3.up * cameraHeight;

        // 목표 회전값 계산 (상대방 Base를 바라보도록 + 45도 아래로 기울기)
        Vector3 lookDirection = opponentBase.transform.position - targetPosition;
        Quaternion horizontalRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0, lookDirection.z)); // 수평 방향만
        Quaternion finalRotation = horizontalRotation * Quaternion.Euler(45, 0, 0); // 45도 아래로 기울기 적용

        // 카메라 즉시 이동 및 회전
        transform.position = targetPosition;
        transform.rotation = finalRotation;

        Debug.Log($"[Camera Log] 카메라가 플레이어 {playerId}의 Base 시점으로 전환되었습니다. (직교 모드)");
    }
}
