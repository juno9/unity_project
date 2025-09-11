using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class NegotiationUI : MonoBehaviour
{
    void Start()
    {
        CreateUI();
    }

    void CreateUI()
    {
        // Ensure there is an EventSystem configured for the new Input System
        if (FindObjectOfType<EventSystem>() == null)
        {
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            var standaloneInputModule = eventSystemGo.GetComponent<StandaloneInputModule>();
            if (standaloneInputModule != null) Destroy(standaloneInputModule);
            eventSystemGo.AddComponent<InputSystemUIInputModule>();
        }

        // Create Canvas
        GameObject canvasGo = new GameObject("NegotiationCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        // Use a simple color background for this scene
        CreateBackgroundImage(canvas.transform);

        // Create Title
        CreateTitle(canvas.transform, "Negotiation");

        // Create Dialogue Text
        CreateDialogueText(canvas.transform, "Will you succeed in the negotiation?");

        // Create Buttons
        CreateButton(canvas.transform, "Success", new Vector2(-150, -50), OnSuccessButtonClick);
        CreateButton(canvas.transform, "Failure", new Vector2(150, -50), OnFailureButtonClick);
    }

    void CreateBackgroundImage(Transform parent)
    {
        GameObject bgGo = new GameObject("BackgroundImage");
        bgGo.transform.SetParent(parent);
        Image bgImage = bgGo.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.3f, 1.0f); // A different color for this scene
        RectTransform rectTransform = bgGo.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = new Vector2(0, 0);
        rectTransform.offsetMax = new Vector2(0, 0);
    }

    void CreateTitle(Transform parent, string titleText)
    {
        GameObject titleGo = new GameObject("TitleText");
        titleGo.transform.SetParent(parent);
        TextMeshProUGUI titleLabel = titleGo.AddComponent<TextMeshProUGUI>();
        titleLabel.text = titleText;
        titleLabel.fontSize = 48;
        titleLabel.color = Color.white;
        titleLabel.alignment = TextAlignmentOptions.Center;
        RectTransform rectTransform = titleGo.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 150);
        rectTransform.sizeDelta = new Vector2(600, 100);
    }

    void CreateDialogueText(Transform parent, string dialogue)
    {
        GameObject textGo = new GameObject("DialogueText");
        textGo.transform.SetParent(parent);
        TextMeshProUGUI textLabel = textGo.AddComponent<TextMeshProUGUI>();
        textLabel.text = dialogue;
        textLabel.fontSize = 32;
        textLabel.color = Color.white;
        textLabel.alignment = TextAlignmentOptions.Center;
        RectTransform rectTransform = textGo.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 50);
        rectTransform.sizeDelta = new Vector2(800, 100);
    }

    void CreateButton(Transform parent, string buttonText, Vector2 position, UnityAction action)
    {
        GameObject buttonGo = new GameObject(buttonText + "Button");
        buttonGo.transform.SetParent(parent);
        Image buttonImage = buttonGo.AddComponent<Image>();
        buttonImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        Button button = buttonGo.AddComponent<Button>();
        button.onClick.AddListener(action);
        RectTransform rectTransform = buttonGo.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(200, 60);

        GameObject textGo = new GameObject("ButtonLabel");
        textGo.transform.SetParent(buttonGo.transform);
        TextMeshProUGUI textLabel = textGo.AddComponent<TextMeshProUGUI>();
        textLabel.text = buttonText;
        textLabel.fontSize = 24;
        textLabel.color = Color.white;
        textLabel.alignment = TextAlignmentOptions.Center;
        RectTransform textRectTransform = textGo.GetComponent<RectTransform>();
        textRectTransform.anchorMin = new Vector2(0, 0);
        textRectTransform.anchorMax = new Vector2(1, 1);
        textRectTransform.offsetMin = new Vector2(0, 0);
        textRectTransform.offsetMax = new Vector2(0, 0);
    }

    void OnSuccessButtonClick()
    {
        Debug.Log("협상 성공! 스테이지 선택 화면으로 돌아갑니다.");
        SceneManager.LoadScene("InGame1");
    }

    void OnFailureButtonClick()
    {
        Debug.Log("협상 결렬! 전투를 시작합니다.");
        SceneManager.LoadScene("SampleScene");
    }
}