using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    public int currentPlayer = 1;
    public Button endTurnButton;
    public GameObject unitInfoPanel;
    public Text unitInfoText;

    private Color player1Color = new Color(0.2f, 0.6f, 1f, 1f); // 파란색
    private Color player2Color = new Color(1f, 0.5f, 0f, 1f); // 주황색

    private List<Unit> player1Units = new List<Unit>();
    private List<Unit> player2Units = new List<Unit>();
    private CameraController cameraController; // 카메라 컨트롤러 참조
    private FogOfWar fogOfWar; // 전장의 안개 참조

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
        Debug.Log("카메라 컨트롤러가 TurnManager에 등록되었습니다.");
    }

    // 전장의 안개 등록
    public void RegisterFogOfWar(FogOfWar fog)
    {
        fogOfWar = fog;
        Debug.Log("전장의 안개가 TurnManager에 등록되었습니다.");
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

        unitInfoPanel.SetActive(false);
        endTurnButton.onClick.AddListener(EndTurn);
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

        // 이동 버튼 색상 업데이트
        if (unitPlacer != null && unitPlacer.moveButton != null)
        {
            unitPlacer.moveButton.GetComponent<Image>().color = new Color(0.3f, 1f, 0.3f, 1f); // 초록색 유지
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

    public void EndTurn()
    {
        Debug.Log("턴 종료 호출됨");
        // 현재 플레이어의 모든 유닛 상태 초기화
        List<Unit> currentPlayerUnits = currentPlayer == 1 ? player1Units : player2Units;
        foreach (Unit unit in currentPlayerUnits)
        {
            unit.ResetTurn();
        }

        // 다음 플레이어로 턴 전환
        int previousPlayer = currentPlayer;
        currentPlayer = currentPlayer == 1 ? 2 : 1;
        
        // 카메라 전환
        Debug.Log($"카메라 전환 시도: cameraController={cameraController != null}, targetPlayer={currentPlayer}");
        if (cameraController != null)
        {
            cameraController.TransitionToPlayerView(currentPlayer);
        }
        else
        {
            Debug.LogWarning("cameraController가 null입니다. 카메라 전환이 작동하지 않습니다.");
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
        
        Debug.Log($"플레이어 {previousPlayer}의 턴이 종료되고 플레이어 {currentPlayer}의 턴이 시작되었습니다.");
    }

    // 상대방 유닛 리스트 반환
    public List<Unit> GetOpponentUnits(int playerId)
    {
        return playerId == 1 ? player2Units : player1Units;
    }
} 