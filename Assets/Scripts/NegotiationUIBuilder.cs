using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // List를 위해 추가
using UnityEngine.EventSystems; // EventSystem을 위해 추가
using UnityEngine.InputSystem.UI; // InputSystemUIInputModule을 위해 추가
using UnityEngine.SceneManagement; // SceneManager를 위해 추가

public class NegotiationUIBuilder : MonoBehaviour
{
    public TMP_FontAsset defaultFontAsset; // 인스펙터에서 할당할 기본 폰트 에셋

    // 요구사항 데이터 구조
    [System.Serializable]
    public class Demand
    {
        public string text;
        [Range(0f, 1f)]
        public float acceptanceProbability; // 0.0 to 1.0
        public bool isAccepted; // 수용 여부
    }

    private List<Demand> demandsList; // 임시 요구사항 목록

    void Start()
    {
        InitializeDemands(); // 요구사항 데이터 초기화
        BuildNegotiationUI();
    }

    void InitializeDemands()
    {
        demandsList = new List<Demand>
        {
            new Demand { text = "영토 일부 할양", acceptanceProbability = 0.7f, isAccepted = false },
            new Demand { text = "자원 1000골드 제공", acceptanceProbability = 0.5f, isAccepted = false },
            new Demand { text = "병력 500명 철수", acceptanceProbability = 0.3f, isAccepted = false },
            new Demand { text = "동맹 체결", acceptanceProbability = 0.9f, isAccepted = false },
            new Demand { text = "기술 공유", acceptanceProbability = 0.2f, isAccepted = false },
            new Demand { text = "무역 협정 체결", acceptanceProbability = 0.8f, isAccepted = false }
        };
    }

    void BuildNegotiationUI()
    {
        // 기존 UI 요소 제거 (선택 사항: 씬에 불필요한 UI가 있다면 수동으로 제거하거나 이 스크립트에서 제거 로직 추가)
        // 예를 들어, 기존 Canvas를 찾아서 제거할 수 있습니다.
        // Canvas existingCanvas = FindObjectOfType<Canvas>();
        // if (existingCanvas != null) Destroy(existingCanvas.gameObject);

        // Canvas 생성
        GameObject canvasGo = new GameObject("NegotiationCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        // 전체 화면 검은색 배경 추가
        GameObject backgroundGo = new GameObject("Background");
        backgroundGo.transform.SetParent(canvas.transform);
        Image backgroundImage = backgroundGo.AddComponent<Image>();
        backgroundImage.color = Color.black; // 검은색 배경
        RectTransform bgRect = backgroundGo.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bgRect.SetAsFirstSibling(); // 가장 뒤에 배치

        // EventSystem 확인 또는 생성 (새로운 Input System 사용 시)
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        string stageName = CurrentStageDataHolder.currentStageName;
        if (string.IsNullOrEmpty(stageName))
        {
            Debug.LogWarning("NegotiationUIBuilder: currentStageName is not set. Using default stage1 for images.");
            stageName = "13C_41.5531°N_60.6236°E"; // Fallback to a default stage name for testing
        }

        // 좌측 지도자 패널
        CreateLeaderPanel(canvas.transform, "LeftLeaderPanel", new Vector2(0, 0), new Vector2(0.3f, 1), TextAlignmentOptions.Left); // anchorMin, anchorMax, alignment

        // 우측 지도자 패널
        CreateLeaderPanel(canvas.transform, "RightLeaderPanel", new Vector2(0.7f, 0), new Vector2(1, 1), TextAlignmentOptions.Right); // anchorMin, anchorMax, alignment

        // 중앙 상황 및 요구사항 패널
        CreateCentralPanel(canvas.transform, "CentralInfoPanel", new Vector2(0.3f, 0), new Vector2(0.7f, 1)); // anchorMin, anchorMax

        // 내비게이션 버튼 생성
        CreateNavigationButtons(canvas.transform);
    }

    void CreateLeaderPanel(Transform parent, string panelName, Vector2 anchorMin, Vector2 anchorMax, TextAlignmentOptions alignment)
    {
        GameObject panelGo = new GameObject(panelName);
        panelGo.transform.SetParent(parent);
        Image panelImage = panelGo.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.7f); // 어두운 반투명 배경

        RectTransform rectTransform = panelGo.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // 지도자 이미지 로드 및 할당
        GameObject leaderImageGo = new GameObject("LeaderImage");
        leaderImageGo.transform.SetParent(panelGo.transform);
        Image leaderImage = leaderImageGo.AddComponent<Image>();

        // AspectRatioFitter 추가
        AspectRatioFitter aspectRatioFitter = leaderImageGo.AddComponent<AspectRatioFitter>();
        aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        aspectRatioFitter.aspectRatio = 1f; // 정방형 이미지이므로 1:1 비율

        // Extract stage number from stageName (e.g., "13C_41.5531°N_60.6236°E" -> "stage1")
        // This assumes stageName format is consistent with StageData in StageSelectUI.cs
        // And that image names are "stageX_playerY"
        string stageName = CurrentStageDataHolder.currentStageName; // Get current stage name here
        string stageNumberString = "";
        if (stageName.StartsWith("13C")) stageNumberString = "stage1";
        else if (stageName.StartsWith("14C")) stageNumberString = "stage2";
        else if (stageName.StartsWith("15C")) stageNumberString = "stage3";
        else if (stageName.StartsWith("16C")) stageNumberString = "stage4";
        else Debug.LogWarning($"NegotiationUIBuilder: Unknown stageName format: {stageName}");

        // Determine player number based on panel name (Left/Right)
        int playerNumber = panelName.Contains("Left") ? 1 : 2;

        string imagePath = $"LeaderPortraits/{stageNumberString}_player{playerNumber}";
        Sprite leaderSprite = Resources.Load<Sprite>(imagePath);

        if (leaderSprite != null)
        {
            leaderImage.sprite = leaderSprite;
            leaderImage.color = Color.white; // 이미지 로드 성공 시 흰색으로
        }
        else
        {
            Debug.LogWarning($"NegotiationUIBuilder: Failed to load leader image at path: {imagePath}. Using gray placeholder.");
            leaderImage.color = Color.gray; // 이미지 로드 실패 시 회색 플레이스홀더
        }

        RectTransform imgRect = leaderImageGo.GetComponent<RectTransform>();
        imgRect.anchorMin = new Vector2(0.1f, 0.1f); // 이미지 위치 조정
        imgRect.anchorMax = new Vector2(0.9f, 0.9f); // 이미지 크기 조정
        imgRect.offsetMin = Vector2.zero;
        imgRect.offsetMax = Vector2.zero;

        // 지도자 이름 및 설명 제거
        // TextMeshProUGUI nameText = CreateText(panelGo.transform, "LeaderName", "지도자 이름", 30, Color.white, alignment);
        // RectTransform nameRect = nameText.GetComponent<RectTransform>();
        // nameRect.anchorMin = new Vector2(0.1f, 0.5f);
        // nameRect.anchorMax = new Vector2(0.9f, 0.6f);
        // nameRect.offsetMin = Vector2.zero;
        // nameRect.offsetMax = Vector2.zero;

        // TextMeshProUGUI descText = CreateText(panelGo.transform, "LeaderDescription", "이곳에 지도자의 상세 설명이 들어갑니다.", 20, Color.white, alignment);
        // RectTransform descRect = descText.GetComponent<RectTransform>();
        // descRect.anchorMin = new Vector2(0.1f, 0.1f);
        // descRect.anchorMax = new Vector2(0.9f, 0.5f);
        // descRect.offsetMin = Vector2.zero;
        // descRect.offsetMax = Vector2.zero;
    }

    void CreateCentralPanel(Transform parent, string panelName, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject panelGo = new GameObject(panelName);
        panelGo.transform.SetParent(parent);
        Image panelImage = panelGo.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.7f); // 중앙 패널 배경색을 어두운 반투명으로 변경

        RectTransform rectTransform = panelGo.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // 현재 상황 제목
        TextMeshProUGUI situationTitle = CreateText(panelGo.transform, "SituationTitle", "현재 상황", 35, Color.yellow, TextAlignmentOptions.Center);
        RectTransform sitTitleRect = situationTitle.GetComponent<RectTransform>();
        sitTitleRect.anchorMin = new Vector2(0.1f, 0.9f);
        sitTitleRect.anchorMax = new Vector2(0.9f, 1.0f);
        sitTitleRect.offsetMin = Vector2.zero;
        sitTitleRect.offsetMax = Vector2.zero;

        // 현재 상황 내용
        TextMeshProUGUI situationContent = CreateText(panelGo.transform, "SituationContent", "여기에 현재 게임 상황에 대한 상세한 설명이 들어갑니다. 예를 들어, 병력 배치, 자원 상황, 주요 이벤트 등이 나열됩니다.", 22, Color.white, TextAlignmentOptions.TopLeft);
        RectTransform sitContentRect = situationContent.GetComponent<RectTransform>();
        sitContentRect.anchorMin = new Vector2(0.1f, 0.5f);
        sitContentRect.anchorMax = new Vector2(0.9f, 0.9f);
        sitContentRect.offsetMin = Vector2.zero;
        sitContentRect.offsetMax = Vector2.zero;

        // 새로운 요구사항 스크롤 뷰 및 아이템 생성
        CreateDemandsScrollView(panelGo.transform, new Vector2(0f, 0f), new Vector2(1f, 0.5f)); // Pass anchors
    }

    void CreateDemandsScrollView(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject scrollViewGo = new GameObject("DemandsScrollView");
        scrollViewGo.transform.SetParent(parent);
        RectTransform scrollRectTransform = scrollViewGo.AddComponent<RectTransform>();
        scrollRectTransform.anchorMin = anchorMin;
        scrollRectTransform.anchorMax = anchorMax;
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = Vector2.zero;

        ScrollRect scrollRect = scrollViewGo.AddComponent<ScrollRect>();
        Image scrollBg = scrollViewGo.AddComponent<Image>();
        scrollBg.color = new Color(0.05f, 0.05f, 0.05f, 0.6f); // 스크롤 뷰 배경

        GameObject viewportGo = new GameObject("Viewport");
        viewportGo.transform.SetParent(scrollViewGo.transform);
        RectTransform viewportRect = viewportGo.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewportGo.AddComponent<Mask>().showMaskGraphic = false;
        viewportGo.AddComponent<Image>().color = Color.clear; // 뷰포트 배경 투명

        GameObject contentGo = new GameObject("Content");
        contentGo.transform.SetParent(viewportGo.transform);
        Image contentImage = contentGo.AddComponent<Image>(); // Content에 Image 컴포넌트 추가
        contentImage.color = Color.clear; // Content 배경색을 투명으로 변경
        RectTransform contentRect = contentGo.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1); // 상단에 고정
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 0); // 너비는 LayoutGroup, 높이는 ContentSizeFitter가 제어
        scrollRect.viewport = viewportRect;
        scrollRect.vertical = true;
        scrollRect.horizontal = false;

        // Add VerticalLayoutGroup to Content
        VerticalLayoutGroup contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(10, 10, 10, 10);
        contentLayout.spacing = 10;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;

        // Add ContentSizeFitter to Content
        ContentSizeFitter contentFitter = contentGo.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRect;

        // 요구사항 아이템 생성
        foreach (var demand in demandsList)
        {
            CreateDemandItem(contentGo.transform, demand);
        }
    }

    void CreateDemandItem(Transform parent, Demand demand)
    {
        // Item Background
        GameObject itemGo = new GameObject("DemandItem");
        itemGo.transform.SetParent(parent, false);
        itemGo.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        ContentSizeFitter itemFitter = itemGo.AddComponent<ContentSizeFitter>();
        itemFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Horizontal Layout for the entire item
        HorizontalLayoutGroup mainLayout = itemGo.AddComponent<HorizontalLayoutGroup>();
        mainLayout.padding = new RectOffset(10, 10, 10, 10);
        mainLayout.spacing = 10;
        mainLayout.childControlWidth = true;
        mainLayout.childControlHeight = false;

        // Left Panel for Texts
        GameObject textPanel = new GameObject("TextPanel");
        textPanel.transform.SetParent(itemGo.transform, false);
        textPanel.AddComponent<LayoutElement>().flexibleWidth = 3; // Takes 3/4 of the space
        VerticalLayoutGroup textLayout = textPanel.AddComponent<VerticalLayoutGroup>();
        textLayout.spacing = 5;

        CreateText(textPanel.transform, "DemandText", demand.text, 20, Color.white, TextAlignmentOptions.Left);
        CreateText(textPanel.transform, "ProbabilityText", $"수용 확률: {demand.acceptanceProbability:P0}", 18, Color.yellow, TextAlignmentOptions.Left);

        // Right Panel for Button
        GameObject buttonPanel = new GameObject("ButtonPanel");
        buttonPanel.transform.SetParent(itemGo.transform, false);
        buttonPanel.AddComponent<LayoutElement>().flexibleWidth = 1; // Takes 1/4 of the space
        VerticalLayoutGroup buttonLayoutGroup = buttonPanel.AddComponent<VerticalLayoutGroup>();
        buttonLayoutGroup.childAlignment = TextAnchor.MiddleCenter;

        // Exchange Button
        GameObject exchangeButtonGo = new GameObject("ExchangeButton");
        exchangeButtonGo.transform.SetParent(buttonPanel.transform, false);
        exchangeButtonGo.AddComponent<Image>().color = new Color(0.3f, 0.5f, 0.3f, 1);
        Button exchangeButton = exchangeButtonGo.AddComponent<Button>();
        exchangeButton.onClick.AddListener(() => {
            Debug.Log($"'{demand.text}' 교환 버튼 클릭! 수용 확률: {demand.acceptanceProbability:P0}");
        });
        exchangeButtonGo.AddComponent<LayoutElement>().preferredHeight = 60;

        CreateButtonLabel(exchangeButtonGo.transform, "ButtonLabel", "교환", 24, Color.white);
    }

    void CreateNavigationButtons(Transform parent)
    {
        // 돌아가기 버튼
        GameObject backButtonGo = new GameObject("BackButton");
        backButtonGo.transform.SetParent(parent);
        Button backButton = backButtonGo.AddComponent<Button>();
        Image backButtonBg = backButtonGo.AddComponent<Image>();
        backButtonBg.color = new Color(0.2f, 0.2f, 0.5f, 1); // 파란색 계열
        RectTransform backButtonRect = backButtonGo.GetComponent<RectTransform>();
        backButtonRect.anchorMin = new Vector2(0.02f, 0.02f); // 위치 조정
        backButtonRect.anchorMax = new Vector2(0.18f, 0.08f); // 크기 조정
        backButtonRect.offsetMin = Vector2.zero;
        backButtonRect.offsetMax = Vector2.zero;

        TextMeshProUGUI backButtonLabel = CreateButtonLabel(backButtonGo.transform, "ButtonLabel", "돌아가기", 20, Color.white);
        backButton.onClick.AddListener(() => {
            Debug.Log("돌아가기 버튼 클릭! InGame1 로드 시도.");
            SceneManager.LoadScene("InGame1"); // 스테이지 선택 화면으로
        });

        // 협상 결렬 버튼
        GameObject failButtonGo = new GameObject("FailButton");
        failButtonGo.transform.SetParent(parent);
        Button failButton = failButtonGo.AddComponent<Button>();
        Image failButtonBg = failButtonGo.AddComponent<Image>();
        failButtonBg.color = new Color(0.5f, 0.2f, 0.2f, 1); // 붉은색 계열
        RectTransform failButtonRect = failButtonGo.GetComponent<RectTransform>();
        failButtonRect.anchorMin = new Vector2(0.82f, 0.02f); // 위치 조정
        failButtonRect.anchorMax = new Vector2(0.98f, 0.08f); // 크기 조정
        failButtonRect.offsetMin = Vector2.zero;
        failButtonRect.offsetMax = Vector2.zero;

        TextMeshProUGUI failButtonLabel = CreateButtonLabel(failButtonGo.transform, "ButtonLabel", "협상 결렬", 20, Color.white);
        failButton.onClick.AddListener(() => {
            Debug.Log("협상 결렬 버튼 클릭! SampleScene 로드 시도.");
            SceneManager.LoadScene("SampleScene"); // SampleScene으로
        });
    }

    TextMeshProUGUI CreateButtonLabel(Transform parent, string name, string text, float fontSize, Color color)
    {
        TextMeshProUGUI tmpText = CreateText(parent, name, text, fontSize, color, TextAlignmentOptions.Center);
        RectTransform textRect = tmpText.gameObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        return tmpText;
    }

    TextMeshProUGUI CreateText(Transform parent, string name, string text, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        GameObject textGo = new GameObject(name);
        textGo.transform.SetParent(parent, false);
        TextMeshProUGUI tmpText = textGo.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.color = color;
        tmpText.alignment = alignment;
        if (defaultFontAsset != null)
        {
            tmpText.font = defaultFontAsset;
        }
        else
        {
            Debug.LogWarning("NegotiationUIBuilder: Default Font Asset is not assigned. Using default TMP font.");
        }

        LayoutElement layoutElement = textGo.AddComponent<LayoutElement>();
        layoutElement.minHeight = 30f; // 고정된 최소 높이 설정

        return tmpText;
    }
}