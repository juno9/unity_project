using UnityEngine;
using UnityEngine.UI;

public class HealthText : MonoBehaviour
{
    private Text healthText;
    private Unit unit;
    private Canvas canvas;
    private RectTransform rectTransform;
    
    private void Start()
    {
        // 캔버스 생성
        GameObject canvasObj = new GameObject("HealthCanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        
        // 캔버스 스케일러 추가
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100f;
        
        // 캔버스 크기 설정
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1, 1);
        
        // 텍스트 오브젝트 생성
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(canvasObj.transform);
        
        // 텍스트 컴포넌트 설정
        healthText = textObj.AddComponent<Text>();
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthText.fontSize = 4;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.color = Color.white;
        
        // 그림자 효과 추가
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(0.2f, -0.2f);
        
        // RectTransform 설정
        rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(3, 2);
        
        // Unit 컴포넌트 가져오기
        unit = GetComponent<Unit>();
        if (unit != null)
        {
            UpdateHealthText();
        }
    }
    
    private void LateUpdate()
    {
        if (unit != null && healthText != null)
        {
            // 체력 텍스트 업데이트
            UpdateHealthText();
            
            // 카메라를 향하도록 회전
            if (Camera.main != null)
            {
                canvas.transform.rotation = Camera.main.transform.rotation;
            }
            
            // 위치 업데이트 (유닛 머리 위)
            canvas.transform.position = transform.position + Vector3.up * 0.5f;
        }
    }
    
    private void UpdateHealthText()
    {
        healthText.text = $"{unit.currentHealth}/{unit.maxHealth}";
    }
} 