using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float moveSpeed = 50f;
    public float zoomSpeed = 25f;
    public float rotationSpeed = 4.0f;
    public float minZoom = 10f;
    public float maxZoom = 60f;
    public float initialHeight = 30f;
    public float initialAngle = 75f;

    private Vector3 cameraTarget; // Camera orbits around this point
    private Vector2 minPanLimit;
    private Vector2 maxPanLimit;
    private Camera cam;
    private Coroutine transitionCoroutine;
    private int currentPlayerIdForCamera = 1;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = false; // Use Perspective camera for better 3D view
        SetupBoundaries();
        SetInitialPosition();
    }

    void Update()
    {
        HandlePanning();
        HandleRotation();
        HandleZoom();
    }

    void SetupBoundaries()
    {
        HexGrid hexGrid = FindFirstObjectByType<HexGrid>();
        if (hexGrid != null)
        {
            cameraTarget = hexGrid.GetMapCenter();
            Bounds gridBounds = hexGrid.GetGridWorldBounds();
            
            // Add a little padding to the boundaries
            float padding = 1f; // Reduce padding to allow camera to reach the edges
            minPanLimit = new Vector2(gridBounds.min.x - padding, gridBounds.min.z - padding);
            maxPanLimit = new Vector2(gridBounds.max.x + padding, gridBounds.max.z + padding);
        }
        else
        {
            // Fallback if no grid is found
            cameraTarget = Vector3.zero;
            minPanLimit = new Vector2(-50, -50);
            maxPanLimit = new Vector2(50, 50);
            Debug.LogWarning("HexGrid not found. Using default camera boundaries.");
        }
    }

    void SetInitialPosition()
    {
        // Set initial position and rotation
        transform.position = cameraTarget + Quaternion.Euler(initialAngle, 0, 0) * (-Vector3.forward * initialHeight);
        transform.LookAt(cameraTarget);
    }

    void HandlePanning()
    {
        // Pan with WASD or Arrow Keys
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        if (moveX != 0 || moveZ != 0)
        {
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

            Vector3 moveDirection = (right * moveX + forward * moveZ).normalized;
            Vector3 newTargetPosition = cameraTarget + moveDirection * moveSpeed * Time.deltaTime;
            
            ApplyPan(newTargetPosition);
        }
        
        // Pan with Middle Mouse Button
        if (Input.GetMouseButton(2))
        {
            float mouseX = -Input.GetAxis("Mouse X") * moveSpeed * 0.05f;
            float mouseY = -Input.GetAxis("Mouse Y") * moveSpeed * 0.05f;

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

            Vector3 moveDirection = (right * mouseX + forward * mouseY);
            Vector3 newTargetPosition = cameraTarget + moveDirection;

            ApplyPan(newTargetPosition);
        }
    }
    
    void ApplyPan(Vector3 newTargetPosition)
    {
        // Clamp the new target position within the boundaries
        newTargetPosition.x = Mathf.Clamp(newTargetPosition.x, minPanLimit.x, maxPanLimit.x);
        newTargetPosition.z = Mathf.Clamp(newTargetPosition.z, minPanLimit.y, maxPanLimit.y);
        
        // Apply the clamped movement
        transform.position += (newTargetPosition - cameraTarget);
        cameraTarget = newTargetPosition;
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            float yaw = Input.GetAxis("Mouse X") * rotationSpeed;
            
            // Get current offset from target
            Vector3 offset = transform.position - cameraTarget;

            // Rotate around the target
            transform.position = cameraTarget + Quaternion.AngleAxis(yaw, Vector3.up) * offset;
            
            // Always look at the target
            transform.LookAt(cameraTarget);
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector3 offset = transform.position - cameraTarget;
            // Adjust zoom speed based on current zoom level for a smoother feel
            float currentZoomSpeed = zoomSpeed * (offset.magnitude / maxZoom);
            float newZoom = Mathf.Clamp(offset.magnitude - scroll * currentZoomSpeed, minZoom, maxZoom);
            transform.position = cameraTarget + offset.normalized * newZoom;
        }
    }

    public void TransitionToPlayerView(Vector3 targetPosition, int playerId)
    {
        currentPlayerIdForCamera = playerId;
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(TransitionToPosition(targetPosition));
    }

    private IEnumerator TransitionToPosition(Vector3 targetPosition)
    {
        float duration = 0.5f;
        float elapsedTime = 0f;
        Vector3 startingPos = cameraTarget;

        while (elapsedTime < duration)
        {
            Vector3 newTarget = Vector3.Lerp(startingPos, targetPosition, (elapsedTime / duration));
            ApplyPan(newTarget);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ApplyPan(targetPosition);
        StartCoroutine(RotateForCurrentPlayer());
    }

    private IEnumerator RotateForCurrentPlayer()
    {
        float targetYAngle = (currentPlayerIdForCamera == 2) ? 180f : 0f;
        
        float duration = 0.4f;
        float elapsedTime = 0f;
        
        float startYAngle = transform.eulerAngles.y;
        float pitch = transform.eulerAngles.x;
        float distance = (transform.position - cameraTarget).magnitude;

        while (elapsedTime < duration)
        {
            float currentYAngle = Mathf.LerpAngle(startYAngle, targetYAngle, elapsedTime / duration);
            Quaternion desiredRot = Quaternion.Euler(pitch, currentYAngle, 0);
            transform.position = cameraTarget + (desiredRot * Vector3.back * distance);
            transform.LookAt(cameraTarget);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Quaternion finalRot = Quaternion.Euler(pitch, targetYAngle, 0);
        transform.position = cameraTarget + (finalRot * Vector3.back * distance);
        transform.LookAt(cameraTarget);
    }
}
