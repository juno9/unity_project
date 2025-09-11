
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI; // Required for the new Input System

public class MainMenuUI : MonoBehaviour
{
    public TMPro.TMP_FontAsset koreanFontAsset;

    void Start()
    {
        if (koreanFontAsset == null)
        {
            Debug.LogError("MainMenuUI: Korean Font Asset is NOT assigned in the Inspector!");
        }
        else
        {
            Debug.Log("MainMenuUI: Korean Font Asset assigned: " + koreanFontAsset.name);
        }
        CreateUI();
    }

    void CreateUI()
    {
        // Ensure there is an EventSystem configured for the new Input System
        if (FindObjectOfType<EventSystem>() == null)
        {
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            // This automatically adds StandaloneInputModule, we need to replace it
            var standaloneInputModule = eventSystemGo.GetComponent<StandaloneInputModule>();
            if (standaloneInputModule != null) {
                Destroy(standaloneInputModule);
            }
            eventSystemGo.AddComponent<InputSystemUIInputModule>();
            Debug.Log("DIAGNOSTIC: EventSystem with InputSystemUIInputModule created.");
        }

        // Create Canvas
        GameObject canvasGo = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        // Create Background Image
        CreateBackgroundImage(canvas.transform);

        // Create Title
        CreateTitle(canvas.transform, "Hexa Tactics");

        // Create Buttons
        CreateButton(canvas.transform, "Start Game", new Vector2(0, 20), OnStartButtonClick);
        CreateButton(canvas.transform, "Options", new Vector2(0, -40), OnOptionsButtonClick);
        CreateButton(canvas.transform, "Exit", new Vector2(0, -100), OnExitButtonClick);
    }

    void CreateBackgroundImage(Transform parent)
    {
        GameObject bgGo = new GameObject("BackgroundImage");
        bgGo.transform.SetParent(parent);

        Image bgImage = bgGo.AddComponent<Image>();

        // --- DIAGNOSTIC STEP ---
        // Try to load as a Texture2D first.
        Texture2D bgTexture = Resources.Load<Texture2D>("UI/Mainescene/Main_background");
        if (bgTexture != null)
        {
            // If the texture is readable, we can create a sprite from it.
            try
            {
                Sprite bgSprite = Sprite.Create(bgTexture, new Rect(0, 0, bgTexture.width, bgTexture.height), new Vector2(0.5f, 0.5f));
                bgImage.sprite = bgSprite;
            }
            catch (UnityException ex)
            {
                Debug.LogError("DIAGNOSTIC: Texture loaded, but FAILED to create Sprite. This often means 'Read/Write Enabled' is not checked in the texture's import settings. Full error: " + ex.Message);
                bgImage.color = Color.blue; // Blue for this specific error
            }
        }
        else
        {
            // If it fails to load even as a Texture2D, the resource is truly not found.
            Debug.LogError("DIAGNOSTIC: FAILED to load even as a Texture2D. The resource cannot be found by Unity. This points to a deep project cache/corruption issue.");
            bgImage.color = Color.red; // Red for this specific error
        }

        // Set anchors and pivots to stretch full screen
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
        titleLabel.font = koreanFontAsset;
        Debug.Log($"MainMenuUI: Title font set to {koreanFontAsset.name}");
        titleLabel.text = titleText;
        titleLabel.fontSize = 48;
        titleLabel.alignment = TextAlignmentOptions.Center;
        titleLabel.fontStyle = FontStyles.Bold;

        RectTransform rectTransform = titleGo.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 150);
        rectTransform.sizeDelta = new Vector2(600, 100);
    }

    void CreateButton(Transform parent, string buttonText, Vector2 position, UnityAction action)
    {
        // Create Button GameObject
        GameObject buttonGo = new GameObject(buttonText + "Button");
        buttonGo.transform.SetParent(parent);

        // Add Image component for the button background
        Image buttonImage = buttonGo.AddComponent<Image>();
        buttonImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark semi-transparent background

        // Add Button component
        Button button = buttonGo.AddComponent<Button>();
        button.onClick.AddListener(action);

        // Set button position and size
        RectTransform rectTransform = buttonGo.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(200, 50);

        // Create Text for the button
        GameObject textGo = new GameObject("ButtonLabel");
        textGo.transform.SetParent(buttonGo.transform);

        TextMeshProUGUI textLabel = textGo.AddComponent<TextMeshProUGUI>();
        textLabel.text = buttonText;
        textLabel.fontSize = 24;
        textLabel.color = Color.white;
        textLabel.alignment = TextAlignmentOptions.Center;
        textLabel.font = koreanFontAsset;
        Debug.Log($"MainMenuUI: ButtonLabel font set to {koreanFontAsset.name}");

        RectTransform textRectTransform = textGo.GetComponent<RectTransform>();
        textRectTransform.anchorMin = new Vector2(0, 0);
        textRectTransform.anchorMax = new Vector2(1, 1);
        textRectTransform.offsetMin = new Vector2(0, 0);
        textRectTransform.offsetMax = new Vector2(0, 0);
    }

    void OnStartButtonClick()
    {
        Debug.Log("DIAGNOSTIC: OnStartButtonClick method successfully called.");
        // Before loading the scene, make sure it's added to the Build Settings.
        Debug.Log("Attempting to load scene: InGame1");
        SceneManager.LoadScene("InGame1");
        Debug.Log("DIAGNOSTIC: The LoadScene command for 'InGame1' has been issued. If the scene does not change, the issue is with Unity's SceneManager or Build Settings.");
    }

    void OnOptionsButtonClick()
    {
        Debug.Log("설정 버튼 클릭!");
        // Options logic would go here.
    }

    void OnExitButtonClick()
    {
        Debug.Log("게임 종료 버튼 클릭!");
        Application.Quit();

#if UNITY_EDITOR
        // This line stops the editor play mode when Application.Quit() is called.
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
