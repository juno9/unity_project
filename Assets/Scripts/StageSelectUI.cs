using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class StageSelectUI : MonoBehaviour
{
    // Data structure for a stage
    public class StageData
    {
        public string stageName;
        public string description;
        public string imageName;
    }

    private List<StageData> stageDataList;

    // UI elements for the right panel
    private GameObject rightPanel;
    private Image stageImage;
    private TextMeshProUGUI descriptionText;
    private Button startNegotiationButton;

    void Start()
    {
        InitializeData();
        CreateUI();
    }

    void InitializeData()
    {
        stageDataList = new List<StageData>
        {
            new StageData 
            {
                stageName = "13C_41.5531°N_60.6236°E",
                imageName = "stage_1",
                description = "<B>상황 배경</B>\n세력 대결: 몽골군 약 15만 명, 호라즘군 약 40만 명(분산 배치로 방어력 약화)으로 숫자는 호라즘 우세이나, 몽골 군대는 철저한 기동전과 전략적 분산 공격으로 우위를 점함.\n\n<B>주요 특징 (게임적 요소)</B>\n- 분할 공격 전략, 기동성과 속도, 심리전 및 전술, 점령 후 자원 확보, 협상과 배신 요소"
            },
            new StageData 
            {
                stageName = "14C_50.25639°N_1.88778°E",
                imageName = "stage_2",
                description = "<B>상황 배경</B>\n수적 열세: 영국군 약 12,000명 대 프랑스군 30,000~40,000명, 수적 불리함이 큰 난관.\n\n<B>주요 특징 (게임적 요소)</B>\n- 진형 방어 전략, 장궁 집중 사격, 프랑스군 반복 돌격, 기사단 패배와 시대 변화, 협상 대체 요소"
            },
            new StageData 
            {
                stageName = "15C_41.030°N_28.935°E",
                imageName = "stage_3",
                description = "<B>상황 배경</B>\n병력 배치: 동로마 제국 약 7,000명 대 오스만 제국 약 10만 명 이상, 수적 열세가 절대적.\n\n<B>주요 특징 (게임적 요소)</B>\n- 균형파괴 수적 불리, 성벽과 해자 활용, 공성 무기 관리, 시간 및 자원 압박, 최후 통첩 시 협상 기회"
            },
            new StageData 
            {
                stageName = "16C_34.76°N_128.43°E",
                imageName = "stage_4",
                description = "<B>상황 배경</B>\n임진왜란 중, 조선 수군이 남해안 제해권을 확보하기 위한 중요한 해전.\n\n<B>주요 특징 (게임적 요소)</B>\n- 함대 대 함대 전투, 학의 날개 진형 사용, 지형 유인 작전, 사기와 자원 관리, 협상 대체 요소"
            }
        };
    }

    void CreateUI()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            var standaloneInputModule = eventSystemGo.GetComponent<StandaloneInputModule>();
            if (standaloneInputModule != null) Destroy(standaloneInputModule);
            eventSystemGo.AddComponent<InputSystemUIInputModule>();
        }

        GameObject canvasGo = new GameObject("StageSelectCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        CreateLeftPanel(canvas.transform);
        CreateRightPanel(canvas.transform);
    }

    void CreateLeftPanel(Transform parent)
    {
        GameObject leftPanel = new GameObject("LeftPanel");
        leftPanel.transform.SetParent(parent);
        RectTransform panelRect = leftPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0.25f, 1);
        panelRect.offsetMin = new Vector2(10, 10);
        panelRect.offsetMax = new Vector2(-5, -10);
        leftPanel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(leftPanel.transform);
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        scrollView.AddComponent<Image>().color = new Color(0, 0, 0, 0.2f);
        RectTransform scrollRectTransform = scrollView.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0, 0);
        scrollRectTransform.anchorMax = new Vector2(1, 1);
        scrollRectTransform.sizeDelta = Vector2.zero;
        scrollRectTransform.anchoredPosition = Vector2.zero;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform);
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        viewport.AddComponent<Image>();
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = new Vector2(-17, 0);
        viewportRect.anchoredPosition = Vector2.zero;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 300);

        VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.spacing = 10;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;

        ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;

        foreach (var stageData in stageDataList)
        {
            CreateStageButton(content.transform, stageData);
        }
    }

    void CreateStageButton(Transform parent, StageData data)
    {
        GameObject buttonGo = new GameObject("StageButton");
        buttonGo.transform.SetParent(parent);
        buttonGo.AddComponent<Image>().color = new Color(0.2f, 0.3f, 0.4f, 1);
        Button button = buttonGo.AddComponent<Button>();
        button.onClick.AddListener(() => DisplayStageInfo(data));
        LayoutElement layout = buttonGo.AddComponent<LayoutElement>();
        layout.minHeight = 40;

        TextMeshProUGUI textLabel = new GameObject("ButtonLabel").AddComponent<TextMeshProUGUI>();
        textLabel.transform.SetParent(buttonGo.transform);
        textLabel.text = data.stageName;
        textLabel.fontSize = 16;
        textLabel.color = Color.white;
        textLabel.alignment = TextAlignmentOptions.Center;
        RectTransform textRect = textLabel.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5, 0);
        textRect.offsetMax = new Vector2(-5, 0);
    }

    void CreateRightPanel(Transform parent)
    {
        rightPanel = new GameObject("RightPanel");
        rightPanel.transform.SetParent(parent);
        RectTransform panelRect = rightPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.25f, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.offsetMin = new Vector2(5, 10);
        panelRect.offsetMax = new Vector2(-10, -10);
        rightPanel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

        GameObject imageGo = new GameObject("StageImage");
        imageGo.transform.SetParent(rightPanel.transform);
        stageImage = imageGo.AddComponent<Image>();
        RectTransform imageRect = imageGo.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0, 0.55f);
        imageRect.anchorMax = new Vector2(1, 1);
        imageRect.offsetMin = new Vector2(15, 15);
        imageRect.offsetMax = new Vector2(-15, -15);
        imageRect.pivot = new Vector2(0.5f, 0.5f);

        AspectRatioFitter fitter = imageGo.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = 1408f / 768f;

        // Create a background panel for the description text
        GameObject descPanelGo = new GameObject("DescriptionPanel");
        descPanelGo.transform.SetParent(rightPanel.transform);
        Image descBgImage = descPanelGo.AddComponent<Image>();
        descBgImage.color = new Color(0, 0, 0, 0.4f);
        RectTransform descPanelRect = descPanelGo.GetComponent<RectTransform>();
        descPanelRect.anchorMin = new Vector2(0, 0);
        descPanelRect.anchorMax = new Vector2(1, 0.55f);
        descPanelRect.offsetMin = new Vector2(15, 80);
        descPanelRect.offsetMax = new Vector2(-15, -15);

        // Create the description text as a child of the panel
        descriptionText = new GameObject("DescriptionText").AddComponent<TextMeshProUGUI>();
        descriptionText.transform.SetParent(descPanelGo.transform);
        descriptionText.enableWordWrapping = true;
        descriptionText.fontSize = 16;
        descriptionText.alignment = TextAlignmentOptions.TopLeft;
        RectTransform textRect = descriptionText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(10, 10); // Padding
        textRect.offsetMax = new Vector2(-10, -10); // Padding

        GameObject negButtonGo = new GameObject("StartNegotiationButton");
        negButtonGo.transform.SetParent(rightPanel.transform);
        negButtonGo.AddComponent<Image>().color = new Color(0.5f, 0.2f, 0.2f, 1);
        startNegotiationButton = negButtonGo.AddComponent<Button>();
        RectTransform buttonRect = negButtonGo.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0);
        buttonRect.anchorMax = new Vector2(0.5f, 0);
        buttonRect.pivot = new Vector2(0.5f, 0);
        buttonRect.sizeDelta = new Vector2(180, 50);
        buttonRect.anchoredPosition = new Vector2(0, 15);

        TextMeshProUGUI negButtonLabel = new GameObject("ButtonLabel").AddComponent<TextMeshProUGUI>();
        negButtonLabel.transform.SetParent(negButtonGo.transform);
        negButtonLabel.text = "협상 시작";
        negButtonLabel.fontSize = 20;
        negButtonLabel.color = Color.white;
        negButtonLabel.alignment = TextAlignmentOptions.Center;
        RectTransform negLabelRect = negButtonLabel.GetComponent<RectTransform>();
        negLabelRect.anchorMin = Vector2.zero;
        negLabelRect.anchorMax = Vector2.one;
        negLabelRect.offsetMin = Vector2.zero;
        negLabelRect.offsetMax = Vector2.zero;

        rightPanel.SetActive(false);
    }

    void DisplayStageInfo(StageData data)
    {
        rightPanel.SetActive(true);
        descriptionText.text = data.description;

        Texture2D stageTexture = Resources.Load<Texture2D>("StageImages/" + data.imageName);
        if (stageTexture != null)
        {
            try
            {
                Sprite stageSprite = Sprite.Create(stageTexture, new Rect(0, 0, stageTexture.width, stageTexture.height), new Vector2(0.5f, 0.5f));
                stageImage.sprite = stageSprite;
                stageImage.color = Color.white;
            }
            catch (UnityException ex)
            {
                Debug.LogError("DIAGNOSTIC: Stage image loaded as texture, but FAILED to create Sprite for '" + data.imageName + "'. Check 'Read/Write Enabled' in import settings. Error: " + ex.Message);
                stageImage.sprite = null;
                stageImage.color = Color.blue; // Blue for this error
            }
        }
        else
        {
            Debug.LogError("DIAGNOSTIC: Stage image FAILED to load as Texture2D: " + data.imageName);
            stageImage.sprite = null;
            stageImage.color = new Color(0, 0, 0, 0.5f); // Dark placeholder
        }

        startNegotiationButton.onClick.RemoveAllListeners();
        startNegotiationButton.onClick.AddListener(() => {
            Debug.Log("협상 시작: " + data.stageName);
            CurrentStageDataHolder.currentStageName = data.stageName;
            SceneManager.LoadScene("InGame2");
        });
    }
}