using UnityEngine;

/// <summary>
/// Debug helper for testing the Game Over functionality.
/// Add this to any GameObject in the scene and use the keyboard shortcuts to test.
/// </summary>
public class GameOverDebugHelper : MonoBehaviour
{
    [Header("Debug Controls")]
    [Tooltip("Press this key to trigger game over instantly")]
    public KeyCode triggerGameOverKey = KeyCode.G;
    
    [Tooltip("Press this key to reduce player lives by 1")]
    public KeyCode reduceLivesKey = KeyCode.L;
    
    [Header("Debug Info")]
    public bool showDebugInfo = true;
    
    private Player player;
    private GameOverMenu gameOverMenu;
    private UIManager uiManager;
    
    void Start()
    {
        // Find components
        player = FindObjectOfType<Player>();
        gameOverMenu = FindObjectOfType<GameOverMenu>();
        uiManager = FindObjectOfType<UIManager>();
        
        if (showDebugInfo)
        {
            Debug.Log($"GameOverDebugHelper initialized:");
            Debug.Log($"- Player found: {player != null}");
            Debug.Log($"- GameOverMenu found: {gameOverMenu != null}");
            Debug.Log($"- UIManager found: {uiManager != null}");
            Debug.Log($"Controls: {triggerGameOverKey} = Game Over, {reduceLivesKey} = Reduce Lives");
        }
    }
    
    void Update()
    {
        // Debug controls
        if (Input.GetKeyDown(triggerGameOverKey))
        {
            TriggerGameOver();
        }
        
        if (Input.GetKeyDown(reduceLivesKey))
        {
            ReducePlayerLives();
        }
    }
    
    void TriggerGameOver()
    {
        if (gameOverMenu != null)
        {
            // Get current score and coins from UI or player
            int currentScore = 1000; // Default test score
            int currentCoins = 50;   // Default test coins
            
            if (player != null)
            {
                // Try to get actual values using reflection (since fields are private)
                var scoreField = typeof(Player).GetField("score", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var coinsField = typeof(Player).GetField("coins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (scoreField != null) currentScore = (int)(float)scoreField.GetValue(player);
                if (coinsField != null) currentCoins = (int)coinsField.GetValue(player);
            }
            
            gameOverMenu.ShowGameOver(currentScore, currentCoins);
            Debug.Log($"Game Over triggered! Score: {currentScore}, Coins: {currentCoins}");
        }
        else if (uiManager != null)
        {
            uiManager.ShowGameOverMenu();
            Debug.Log("Game Over triggered via UIManager!");
        }
        else
        {
            Debug.LogWarning("No Game Over system found!");
        }
    }
    
    void ReducePlayerLives()
    {
        if (player != null)
        {
            // Simulate hitting an obstacle
            var hitMethod = typeof(Player).GetMethod("HitObstacles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (hitMethod != null)
            {
                hitMethod.Invoke(player, null);
                Debug.Log("Player hit obstacle (debug)");
            }
            else
            {
                Debug.LogWarning("Could not find HitObstacles method");
            }
        }
        else
        {
            Debug.LogWarning("No Player found!");
        }
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Game Over Debug Helper", GUI.skin.box);
        GUILayout.Label($"Press {triggerGameOverKey} - Trigger Game Over");
        GUILayout.Label($"Press {reduceLivesKey} - Reduce Lives");
        
        if (player != null)
        {
            // Try to show current lives using reflection
            var livesField = typeof(Player).GetField("currentLives", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (livesField != null)
            {
                int lives = (int)livesField.GetValue(player);
                GUILayout.Label($"Current Lives: {lives}");
            }
        }
        
        GUILayout.EndArea();
    }
}

// Extension to make this script editor-friendly
#if UNITY_EDITOR
[System.Serializable]
public class GameOverDebugHelperEditor
{
    [UnityEditor.MenuItem("GameObject/UI/Game Over Debug Helper")]
    static void CreateGameOverDebugHelper()
    {
        GameObject go = new GameObject("GameOverDebugHelper");
        go.AddComponent<GameOverDebugHelper>();
        UnityEditor.Selection.activeGameObject = go;
    }
}
#endif
