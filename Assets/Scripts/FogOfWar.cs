using UnityEngine;
using System.Collections.Generic;

public class FogOfWar : MonoBehaviour
{
    [SerializeField] private Material fogMaterial;
    [SerializeField] private float fogRadius = 3f; // 유닛의 시야 범위
    [SerializeField] private float fogUpdateInterval = 0.5f; // 안개 업데이트 주기
    
    private RenderTexture fogTexture;
    private Dictionary<Unit, Vector2> unitPositions = new Dictionary<Unit, Vector2>();
    private float nextUpdateTime;

    void Start()
    {
        InitializeFogTexture();
    }

    void InitializeFogTexture()
    {
        // 안개 텍스처 초기화
        fogTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        fogTexture.Create();
        
        // 안개 머티리얼에 텍스처 할당
        if (fogMaterial != null)
        {
            fogMaterial.SetTexture("_FogTexture", fogTexture);
        }
    }

    void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateFog();
            nextUpdateTime = Time.time + fogUpdateInterval;
        }
    }

    void UpdateFog()
    {
        // 모든 유닛의 위치를 가져와서 안개 업데이트
        unitPositions.Clear();
        foreach (Unit unit in FindObjectsOfType<Unit>())
        {
            if (unit != null)
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
                unitPositions[unit] = screenPos;
            }
        }

        // 안개 텍스처 업데이트
        UpdateFogTexture();
    }

    void UpdateFogTexture()
    {
        // 안개 텍스처를 업데이트하는 로직
        // 여기서는 간단한 원형 안개를 구현
        RenderTexture.active = fogTexture;
        GL.Clear(true, true, Color.black);

        foreach (var unitPos in unitPositions.Values)
        {
            DrawFogCircle(unitPos, fogRadius);
        }

        RenderTexture.active = null;
    }

    void DrawFogCircle(Vector2 center, float radius)
    {
        // 원형 안개를 그리는 로직
        // 실제 구현에서는 셰이더를 사용하여 더 효율적으로 구현할 수 있습니다
    }

    void OnDestroy()
    {
        if (fogTexture != null)
        {
            fogTexture.Release();
        }
    }
} 