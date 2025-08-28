using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    public int currentPlayer = 1;
    public Button endTurnButton;
    public GameObject unitInfoPanel;
    public Text unitInfoText;
    public Text timerText; // 타이머 UI
    public Text guideText; // 안내문구 UI

    [Header("행동 코스트(AP)")]
    public Text apText; // AP 표시 UI
    private int player1AP;
    private int player2AP;
    public const int MAX_AP = 10;
    public const int UNIT_PLACEMENT_COST = 3;
    public const int MOVE_COST = 1;
    public const int ATTACK_MOVE_COST = 2;

    private Color player1Color = new Color(0.2f, 0.6f, 1f, 1f); // 파란색
    private Color player2Color = new Color(1f, 0.5f, 0f, 1f); // 주황색

    private List<Unit> player1Units = new List<Unit>();
    private List<Unit> player2Units = new List<Unit>();
    private Base player1Base;
    private Base player2Base;
    private CameraController cameraController; // 카메라 컨트롤러 참조
    private FogOfWar fogOfWar; // 전장의 안개 참조
    private Coroutine turnTimerCoroutine; // 턴 타이머 코루틴

    private CameraController CamController
    {
        get
        {
            if (cameraController == null)
            {
                cameraController = FindObjectOfType<CameraController>();
            }
            return cameraController;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CreateUI();
    }

    // 카메라 컨트롤러 등록
    public void RegisterCameraController(CameraController controller)
    {
        cameraController = controller;
        
    }

    // 전장의 안개 등록
    public void RegisterFogOfWar(FogOfWar fog)
    {
        fogOfWar = fog;
        
    }

    private void CreateUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 턴 종료 버튼 생성
        GameObject buttonObj = new GameObject("EndTurnButton");
        buttonObj.transform.SetParent(canvas.transform);
        endTurnButton = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = player1Color;
        
        RectTransform rt = buttonObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(150, 70);
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(0, 300);

        // 버튼 텍스트
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = "턴 종료";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        // 유닛 정보 패널 생성
        GameObject panelObj = new GameObject("UnitInfoPanel");
        panelObj.transform.SetParent(canvas.transform);
        unitInfoPanel = panelObj;
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform panelRT = panelObj.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(200, 150);
        panelRT.anchorMin = new Vector2(0, 1);
        panelRT.anchorMax = new Vector2(0, 1);
        panelRT.pivot = new Vector2(0, 1);
        panelRT.anchoredPosition = new Vector2(20, -20);

        // 유닛 정보 텍스트
        GameObject infoTextObj = new GameObject("UnitInfoText");
        infoTextObj.transform.SetParent(panelObj.transform);
        unitInfoText = infoTextObj.AddComponent<Text>();
        unitInfoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        unitInfoText.alignment = TextAnchor.UpperLeft;
        unitInfoText.color = Color.white;
        unitInfoText.fontSize = 14;
        RectTransform infoTextRT = infoTextObj.GetComponent<RectTransform>();
        infoTextRT.anchorMin = Vector2.zero;
        infoTextRT.anchorMax = Vector2.one;
        infoTextRT.offsetMin = new Vector2(10, 10);
        infoTextRT.offsetMax = new Vector2(-10, -10);

        // 타이머 텍스트 생성
        GameObject timerObj = new GameObject("TimerText");
        timerObj.transform.SetParent(canvas.transform);
        timerText = timerObj.AddComponent<Text>();
        timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        timerText.fontSize = 40;
        timerText.alignment = TextAnchor.MiddleCenter;
        RectTransform timerRT = timerObj.GetComponent<RectTransform>();
        timerRT.sizeDelta = new Vector2(200, 100);
        timerRT.anchorMin = new Vector2(0.5f, 1);
        timerRT.anchorMax = new Vector2(0.5f, 1);
        timerRT.pivot = new Vector2(0.5f, 1);
        timerRT.anchoredPosition = new Vector2(0, -20);

        // 안내문구 텍스트 생성
        GameObject guideObj = new GameObject("GuideText");
        guideObj.transform.SetParent(canvas.transform, false);
        guideText = guideObj.AddComponent<Text>();
        guideText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        guideText.fontSize = 32;
        guideText.color = Color.yellow;
        guideText.alignment = TextAnchor.UpperCenter;
        Shadow shadow = guideObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(2, -2);
        RectTransform guideRT = guideObj.GetComponent<RectTransform>();
        guideRT.anchorMin = new Vector2(0.5f, 1f);
        guideRT.anchorMax = new Vector2(0.5f, 1f);
        guideRT.pivot = new Vector2(0.5f, 1f);
        guideRT.anchoredPosition = new Vector2(0, -150);
        guideRT.sizeDelta = new Vector2(Screen.width * 0.8f, 100);
        guideObj.SetActive(false); // 초기에는 비활성화

        // AP 텍스트 생성
        GameObject apTextObj = new GameObject("APText");
        apTextObj.transform.SetParent(canvas.transform, false);
        apText = apTextObj.AddComponent<Text>();
        apText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        apText.fontSize = 28;
        apText.color = Color.green;
        apText.alignment = TextAnchor.MiddleLeft;
        RectTransform apRT = apTextObj.GetComponent<RectTransform>();
        apRT.anchorMin = new Vector2(0, 1);
        apRT.anchorMax = new Vector2(0, 1);
        apRT.pivot = new Vector2(0, 1);
        apRT.anchoredPosition = new Vector2(20, -200);
        apRT.sizeDelta = new Vector2(200, 50);

        unitInfoPanel.SetActive(false);
        endTurnButton.onClick.AddListener(EndTurn);
    }

    public void StartFirstTurn()
    {
        currentPlayer = 1;
        player1AP = MAX_AP;
        player2AP = MAX_AP;
        UpdateAPUI();
        
        if (CamController != null)
        {
            if (player1Base != null)
            {
                CamController.TransitionToPlayerView(player1Base.transform.position, 1);
            }
        }
        
        if (fogOfWar != null)
        {
            fogOfWar.OnPlayerTurnChanged(currentPlayer);
        }
        
        UpdateButtonColors();
        StartTurnTimer();
        
    }

    private void StartTurnTimer()
    {
        if (turnTimerCoroutine != null)
        {
            StopCoroutine(turnTimerCoroutine);
        }
        turnTimerCoroutine = StartCoroutine(TurnTimer());
    }

    private IEnumerator TurnTimer()
    {
        float timeLeft = 30f;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            timerText.text = $"{Mathf.CeilToInt(timeLeft)}";
            yield return null;
        }
        EndTurn();
    }

    public void UpdateFogOfWar()
    {
        if (fogOfWar != null)
        {
            fogOfWar.OnPlayerTurnChanged(currentPlayer);
        }
    }

    public void UpdateButtonColors()
    {
        Color currentColor = currentPlayer == 1 ? player1Color : player2Color;
        
        // 턴 종료 버튼 색상 업데이트
        if (endTurnButton != null)
        {
            endTurnButton.GetComponent<Image>().color = currentColor;
        }

        // 유닛 배치 버튼 색상 업데이트
        UnitPlacer unitPlacer = FindFirstObjectByType<UnitPlacer>();
        if (unitPlacer != null && unitPlacer.unitPlacementButton != null)
        {
            unitPlacer.unitPlacementButton.GetComponent<Image>().color = currentColor;
        }

        // 공격 버튼 색상 업데이트
        if (unitPlacer != null && unitPlacer.attackButton != null)
        {
            unitPlacer.attackButton.GetComponent<Image>().color = new Color(1f, 0.3f, 0.3f, 1f); // 빨간색 유지
        }

        
    }

    public void RegisterUnit(Unit unit)
    {
        if (unit.playerId == 1)
            player1Units.Add(unit);
        else
            player2Units.Add(unit);
    }

    public void UnregisterUnit(Unit unit)
    {
        if (unit.playerId == 1)
            player1Units.Remove(unit);
        else
            player2Units.Remove(unit);
    }

    public void RegisterBase(Base b)
    {
        if (b.playerId == 1)
        {
            player1Base = b;
        }
        else
        {
            player2Base = b;
        }
    }

    public void ShowUnitInfo(Unit unit)
    {
        if (unit == null)
        {
            unitInfoPanel.SetActive(false);
            return;
        }

        unitInfoPanel.SetActive(true);
        unitInfoText.text = $"플레이어 {unit.playerId} 유닛\n" +
                           $"체력: {unit.currentHealth}/{unit.maxHealth}\n" +
                           $"공격력: {unit.attackPower}\n" +
                           $"이동력: {unit.moveRange}\n" +
                           $"이동 가능: {(unit.hasMoved ? "불가" : "가능")}\n" +
                           $"공격 가능: {(unit.hasAttacked ? "불가" : "가능")}";
    }

    public void ShowBaseInfo(Base b)
    {
        if (b == null)
        {
            unitInfoPanel.SetActive(false);
            return;
        }

        unitInfoPanel.SetActive(true);
        unitInfoText.text = $"플레이어 {b.playerId} 거점";
    }

    public void EndTurn()
    {
        ShowUnitInfo(null);
        
        if (turnTimerCoroutine != null)
        {
            StopCoroutine(turnTimerCoroutine);
        }

        // 현재 플레이어의 모든 유닛 상태 초기화
        List<Unit> currentPlayerUnits = currentPlayer == 1 ? player1Units : player2Units;
        foreach (Unit unit in currentPlayerUnits)
        {
            unit.ResetTurn();
        }

        // 다음 플레이어로 턴 전환
        int previousPlayer = currentPlayer;
        currentPlayer = currentPlayer == 1 ? 2 : 1;
        ResetAP();
        
        // 카메라 전환
        
        if (CamController != null)
        {
            Base nextPlayerBase = (currentPlayer == 1) ? player1Base : player2Base;
            if (nextPlayerBase != null)
            {
                CamController.TransitionToPlayerView(nextPlayerBase.transform.position, currentPlayer);
            }
        }
        else
        {
            Debug.LogWarning("[Camera Log] cameraController가 null입니다. 카메라 전환이 작동하지 않습니다.");
        }
        
        // 전장의 안개 업데이트
        if (fogOfWar != null)
        {
            fogOfWar.OnPlayerTurnChanged(currentPlayer);
        }
        else
        {
            Debug.LogWarning("fogOfWar가 null입니다. 전장의 안개가 업데이트되지 않습니다.");
        }
        
        // 버튼 색상 업데이트
        UpdateButtonColors();
        StartTurnTimer();
        
        
    }

    public bool SpendAP(int amount)
    {
        if (currentPlayer == 1)
        {
            if (player1AP >= amount)
            {
                player1AP -= amount;
                UpdateAPUI();
                return true;
            }
        }
        else
        {
            if (player2AP >= amount)
            {
                player2AP -= amount;
                UpdateAPUI();
                return true;
            }
        }
        return false;
    }

    private void UpdateAPUI()
    {
        int currentAP = (currentPlayer == 1) ? player1AP : player2AP;
        apText.text = $"AP: {currentAP} / {MAX_AP}";
    }

    private void ResetAP()
    {
        if (currentPlayer == 1)
        {
            player1AP = MAX_AP;
        }
        else
        {
            player2AP = MAX_AP;
        }
        UpdateAPUI();
    }

    // 상대방 유닛 리스트 반환
    public List<Unit> GetOpponentUnits(int playerId)
    {
        return playerId == 1 ? player2Units : player1Units;
    }
}