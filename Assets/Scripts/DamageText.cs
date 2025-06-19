using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageText : MonoBehaviour
{
    public static DamageText Instance { get; private set; }
    
    private Canvas worldCanvas;
    private Camera mainCamera;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        mainCamera = Camera.main;
        CreateWorldCanvas();
    }
    
    private void CreateWorldCanvas()
    {
        // 월드 스페이스 캔버스 생성
        GameObject canvasObj = new GameObject("WorldCanvas");
        canvasObj.transform.SetParent(transform);
        worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.worldCamera = mainCamera;
        
        // 캔버스 스케일러 추가
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100f;
        
        // 캔버스 크기와 위치 설정
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1, 1);
    }
    
    public void ShowDamageText(int damage, Vector3 position, Color textColor = default)
    {
        if (textColor == default)
            textColor = Color.red;
            
        StartCoroutine(AnimateDamageText(damage, position, textColor));
    }
    
    private IEnumerator AnimateDamageText(int damage, Vector3 position, Color textColor)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) yield break;
        }

        // 텍스트 오브젝트 생성
        GameObject textObj = new GameObject("DamageText");
        textObj.transform.SetParent(worldCanvas.transform);
        
        // 텍스트 컴포넌트 추가
        Text damageText = textObj.AddComponent<Text>();
        damageText.text = damage.ToString();
        damageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        damageText.fontSize = 25;
        damageText.color = textColor;
        damageText.alignment = TextAnchor.MiddleCenter;
        damageText.fontStyle = FontStyle.Bold;
        
        // 그림자 효과 추가
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(1, -1);
        
        // RectTransform 설정
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 50);
        
        // 시작 위치 설정 (유닛 머리 위)
        Vector3 startPos = position + Vector3.up * 2f;
        textObj.transform.position = startPos;
        
        // 애니메이션
        float duration = 1.0f;
        float elapsed = 0f;
        Vector3 endPos = startPos + Vector3.up * 1f;
        Color startColor = textColor;
        Color endColor = new Color(textColor.r, textColor.g, textColor.b, 0f);
        
        while (elapsed < duration)
        {
            if (mainCamera == null) break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.Sin(t * Mathf.PI * 0.5f);
            
            // 위치 업데이트
            textObj.transform.position = Vector3.Lerp(startPos, endPos, smoothT);
            
            // 카메라를 향하도록 회전
            textObj.transform.rotation = mainCamera.transform.rotation;
            
            // 색상 페이드 아웃
            damageText.color = Color.Lerp(startColor, endColor, smoothT);
            
            yield return null;
        }
        
        Destroy(textObj);
    }
} 