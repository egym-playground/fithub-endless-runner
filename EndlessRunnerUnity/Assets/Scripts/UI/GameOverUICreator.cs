using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script to automatically create a Game Over UI if one doesn't exist.
/// Attach this to a Canvas in your scene to auto-generate the game over menu.
/// </summary>
public class GameOverUICreator : MonoBehaviour
{
    [Header("Auto-Create Game Over UI")]
    public bool createOnStart = true;
    public bool useSimpleLayout = true;
    
    [Header("UI Styling")]
    public Color backgroundColor = new Color(0, 0, 0, 0.8f);
    public Color textColor = Color.white;
    public Color buttonColor = new Color(0.2f, 0.6f, 1f);
    
    void Start()
    {
        if (createOnStart && FindObjectOfType<GameOverMenu>() == null)
        {
            CreateGameOverUI();
        }
    }
    
    public void CreateGameOverUI()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("GameOverUICreator must be attached to a Canvas!");
            return;
        }
        
        // Create the main game over panel
        GameObject gameOverPanel = CreateGameOverPanel(canvas.transform);
        
        // Add the GameOverMenu component
        GameOverMenu gameOverMenu = FindObjectOfType<GameOverMenu>();
        if (gameOverMenu == null)
        {
            gameOverMenu = gameObject.AddComponent<GameOverMenu>();
        }
        
        // Assign references
        SetupGameOverMenuReferences(gameOverMenu, gameOverPanel);
        
        Debug.Log("Game Over UI created successfully!");
    }
    
    GameObject CreateGameOverPanel(Transform parent)
    {
        // Main panel
        GameObject panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(parent, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = backgroundColor;
        
        // Content container
        GameObject content = new GameObject("Content");
        content.transform.SetParent(panel.transform, false);
        
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(400, 300);
        contentRect.anchoredPosition = Vector2.zero;
        
        VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 20;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = false;
        
        ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Create UI elements
        CreateTitle(content.transform, "Game Over");
        CreateScoreText(content.transform, "Final Score: 0");
        CreateCoinsText(content.transform, "Coins: 0");
        CreateRestartButton(content.transform);
        CreateMainMenuButton(content.transform);
        
        panel.SetActive(false);
        return panel;
    }
    
    void CreateTitle(Transform parent, string text)
    {
        GameObject titleObj = new GameObject("GameOverTitle");
        titleObj.transform.SetParent(parent, false);
        
        Text title = titleObj.AddComponent<Text>();
        title.text = text;
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 36;
        title.color = textColor;
        title.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(400, 60);
    }
    
    void CreateScoreText(Transform parent, string text)
    {
        GameObject scoreObj = new GameObject("FinalScoreText");
        scoreObj.transform.SetParent(parent, false);
        
        Text scoreText = scoreObj.AddComponent<Text>();
        scoreText.text = text;
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = 24;
        scoreText.color = textColor;
        scoreText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.sizeDelta = new Vector2(400, 40);
    }
    
    void CreateCoinsText(Transform parent, string text)
    {
        GameObject coinsObj = new GameObject("FinalCoinsText");
        coinsObj.transform.SetParent(parent, false);
        
        Text coinsText = coinsObj.AddComponent<Text>();
        coinsText.text = text;
        coinsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        coinsText.fontSize = 24;
        coinsText.color = textColor;
        coinsText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform coinsRect = coinsObj.GetComponent<RectTransform>();
        coinsRect.sizeDelta = new Vector2(400, 40);
    }
    
    void CreateRestartButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("RestartButton");
        buttonObj.transform.SetParent(parent, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = buttonColor;
        
        Button button = buttonObj.AddComponent<Button>();
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(200, 50);
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = "Restart";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 20;
        buttonText.color = textColor;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    void CreateMainMenuButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("MainMenuButton");
        buttonObj.transform.SetParent(parent, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(buttonColor.r * 0.8f, buttonColor.g * 0.8f, buttonColor.b * 0.8f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(200, 50);
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = "Main Menu";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 20;
        buttonText.color = textColor;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    void SetupGameOverMenuReferences(GameOverMenu gameOverMenu, GameObject gameOverPanel)
    {
        gameOverMenu.gameOverPanel = gameOverPanel;
        gameOverMenu.gameOverTitle = gameOverPanel.transform.Find("Content/GameOverTitle").GetComponent<Text>();
        gameOverMenu.finalScoreText = gameOverPanel.transform.Find("Content/FinalScoreText").GetComponent<Text>();
        gameOverMenu.finalCoinsText = gameOverPanel.transform.Find("Content/FinalCoinsText").GetComponent<Text>();
        gameOverMenu.restartButton = gameOverPanel.transform.Find("Content/RestartButton").GetComponent<Button>();
        gameOverMenu.mainMenuButton = gameOverPanel.transform.Find("Content/MainMenuButton").GetComponent<Button>();
    }
}
