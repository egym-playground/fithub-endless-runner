using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    [Header("Game Over UI Elements")]
    public GameObject gameOverPanel;
    public Text gameOverTitle;
    public Text finalScoreText;
    public Text finalCoinsText;
    public Button restartButton;
    public Button mainMenuButton;
    
    [Header("Animation Settings")]
    public bool useAnimation = true;
    public float animationDuration = 0.5f;
    
    void Start()
    {
        // Ensure the game over panel is hidden at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        // Setup button listeners
        SetupButtons();
    }
    
    void SetupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }
    
    public void ShowGameOver(int finalScore, int finalCoins)
    {
        if (gameOverPanel == null) return;
        
        // Update UI texts
        UpdateGameOverTexts(finalScore, finalCoins);
        
        // Show the panel
        gameOverPanel.SetActive(true);
        
        // Play animation if enabled
        if (useAnimation)
        {
            PlayShowAnimation();
        }
        
        // Pause the game (optional)
        Time.timeScale = 0f;
    }
    
    void UpdateGameOverTexts(int score, int coins)
    {
        if (gameOverTitle != null)
            gameOverTitle.text = "Game Over";
        
        if (finalScoreText != null)
            finalScoreText.text = "Final Score: " + score.ToString();
        
        if (finalCoinsText != null)
            finalCoinsText.text = "Coins Collected: " + coins.ToString();
    }
    
    void PlayShowAnimation()
    {
        // Simple scale animation using coroutine (no external dependencies)
        if (gameOverPanel != null)
        {
            gameOverPanel.transform.localScale = Vector3.zero;
            StartCoroutine(ScaleAnimation());
        }
    }
    
    System.Collections.IEnumerator ScaleAnimation()
    {
        float elapsedTime = 0;
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / animationDuration;
            
            // Ease out back effect
            progress = 1f - Mathf.Pow(1f - progress, 3f);
            
            gameOverPanel.transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }
        
        gameOverPanel.transform.localScale = targetScale;
    }
    
    public void RestartGame()
    {
        // Resume time scale
        Time.timeScale = 1f;
        
        // Reload the current scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
    
    public void GoToMainMenu()
    {
        // Resume time scale
        Time.timeScale = 1f;
        
        // Load the main menu scene
        SceneManager.LoadScene("Initial");
    }
    
    public void HideGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        // Resume the game
        Time.timeScale = 1f;
    }
    
    void OnDestroy()
    {
        // Ensure time scale is reset when this object is destroyed
        Time.timeScale = 1f;
    }
}
