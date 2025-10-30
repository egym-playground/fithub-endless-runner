using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {

    public Image[] lives;
    public Text coinText;
    public Text scoreText;
    
    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public Text finalCoinsText;
    public Button restartButton;
    public Button mainMenuButton;
    
    void Start()
    {
        Debug.Log("UIManager Start() called");
        
        // Hide game over panel at start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            Debug.Log("Game Over Panel found and hidden at start");
        }
        else
        {
            Debug.LogWarning("Game Over Panel is not assigned in UIManager! Please assign it in the inspector.");
        }
        
        // Setup button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
            
        Debug.Log($"UIManager initialized - GameOverPanel: {gameOverPanel != null}, RestartButton: {restartButton != null}, MainMenuButton: {mainMenuButton != null}");
    }
    
    public void UpdateLives(int currentLives) {
        for (int i = 0; i < lives.Length; i++) {
            lives[i].color = currentLives > i ? Color.white : Color.black;
        }
    }

    public void UpdateCoins(int coins) {
        coinText.text = coins.ToString();
    }

    public void UpdateScore(int score) {
        scoreText.text = score.ToString();
    }
    
    public void ShowGameOverMenu()
    {
        Debug.Log("ShowGameOverMenu() called!");
        
        if (gameOverPanel != null)
        {
            Debug.Log("Game Over Panel found - activating it!");
            
            // Update final scores before showing
            if (finalScoreText != null)
                finalScoreText.text = scoreText.text;
            
            if (finalCoinsText != null)
                finalCoinsText.text = coinText.text;
            
            // Start the slide-down animation
            StartCoroutine(AnimateGameOverFromTop());
                
            Debug.Log($"Game Over Menu displayed! Final Score: {scoreText.text}, Coins: {coinText.text}");
        }
        else
        {
            Debug.LogError("Game Over Panel is NULL! Please assign the gameOverPanel in the UIManager inspector.");
        }
    }
    
    public void RestartGame()
    {
        // Restart the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void GoToMainMenu()
    {
        // Load the Initial scene (main menu)
        SceneManager.LoadScene("Initial");
    }
    
    // Debug method to manually test game over - call this from console or attach to a button
    [System.Obsolete("This is for debugging only")]
    public void DebugTriggerGameOver()
    {
        Debug.Log("DEBUG: Manually triggering game over!");
        ShowGameOverMenu();
    }
    
    void Update()
    {
        // Quick debug: Press 'O' key to trigger game over for testing
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("DEBUG KEY PRESSED: Triggering Game Over");
            ShowGameOverMenu();
        }
    }
    
    System.Collections.IEnumerator AnimateGameOverFromTop()
    {
        if (gameOverPanel == null) yield break;
        
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            Debug.LogWarning("Game Over Panel doesn't have a RectTransform! Using simple SetActive instead.");
            gameOverPanel.SetActive(true);
            yield break;
        }
        
        // Enable the panel but position it above the screen
        gameOverPanel.SetActive(true);
        
        // Store the original position
        Vector2 originalPosition = panelRect.anchoredPosition;
        
        // Calculate the starting position (above the screen)
        float panelHeight = panelRect.rect.height;
        Vector2 startPosition = new Vector2(originalPosition.x, originalPosition.y + Screen.height + panelHeight);
        
        // Set initial position
        panelRect.anchoredPosition = startPosition;
        
        // Animation settings
        float animationDuration = 0.8f;
        float elapsedTime = 0f;
        
        Debug.Log($"Starting slide animation from Y:{startPosition.y} to Y:{originalPosition.y}");
        
        // Animate the slide down
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
            
            // Calculate progress with easing (ease out)
            float progress = elapsedTime / animationDuration;
            progress = 1f - Mathf.Pow(1f - progress, 3f); // Ease out cubic
            
            // Interpolate position
            Vector2 currentPosition = Vector2.Lerp(startPosition, originalPosition, progress);
            panelRect.anchoredPosition = currentPosition;
            
            yield return null;
        }
        
        // Ensure final position is exact
        panelRect.anchoredPosition = originalPosition;
        
        Debug.Log("Game Over slide animation completed!");
    }
}
