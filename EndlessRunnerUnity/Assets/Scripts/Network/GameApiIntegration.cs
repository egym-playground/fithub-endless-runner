using UnityEngine;

/// <summary>
/// Integrates the HTTP client with the game's UI and game over system.
/// This script connects the API functionality to the game flow.
/// </summary>
public class GameApiIntegration : MonoBehaviour
{
    [Header("References")]
    public UIManager uiManager;
    public Player player;
    
    [Header("API Settings")]
    public bool enableOnlineFeatures = true;
    public bool submitScoreOnGameOver = true;
    public bool loadLeaderboardOnStart = true;
    
    private GameApiManager apiManager;
    private HttpClient httpClient;
    
    void Start()
    {
        SetupApiComponents();
        
        if (enableOnlineFeatures && loadLeaderboardOnStart)
        {
            LoadLeaderboardData();
        }
    }
    
    void SetupApiComponents()
    {
        // Find or create API manager
        apiManager = FindObjectOfType<GameApiManager>();
        if (apiManager == null && enableOnlineFeatures)
        {
            GameObject apiObject = new GameObject("GameApiManager");
            apiManager = apiObject.AddComponent<GameApiManager>();
            Debug.Log("GameApiManager created automatically");
        }
        
        // Find UI and Player references if not assigned
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
        
        if (player == null)
            player = FindObjectOfType<Player>();
    }
    
    void LoadLeaderboardData()
    {
        if (apiManager == null) return;
        
        Debug.Log("Loading leaderboard data...");
        
        apiManager.GetLeaderboard(
            onSuccess: (leaderboardData) => {
                Debug.Log($"Leaderboard loaded successfully! {leaderboardData.Length} entries");
                // You can update UI here with leaderboard data
                DisplayLeaderboard(leaderboardData);
            },
            onError: (error) => {
                Debug.LogWarning($"Failed to load leaderboard: {error}");
                // Show offline mode or cached data
            }
        );
    }
    
    void DisplayLeaderboard(LeaderboardEntry[] entries)
    {
        Debug.Log("=== LEADERBOARD ===");
        for (int i = 0; i < Mathf.Min(5, entries.Length); i++)
        {
            var entry = entries[i];
            Debug.Log($"{entry.rank}. {entry.playerName} - {entry.score} pts");
        }
        Debug.Log("==================");
    }
    
    /// <summary>
    /// Call this method when the game ends to submit the score
    /// </summary>
    public void OnGameOver(int finalScore, int totalCoins)
    {
        if (!enableOnlineFeatures || !submitScoreOnGameOver || apiManager == null)
            return;
        
        Debug.Log($"Game Over! Submitting score: {finalScore} points, {totalCoins} coins");
        
        apiManager.OnGameEnd(finalScore, totalCoins);
    }
    
    /// <summary>
    /// Example method to test a simple API call
    /// </summary>
    public void TestSimpleApiCall()
    {
        // Create a simple HttpClient for one-off requests
        GameObject tempObject = new GameObject("TempHttpClient");
        HttpClient client = tempObject.AddComponent<HttpClient>();
        
        // Configure for a test API (you can use any public API for testing)
        client.baseUrl = "https://jsonplaceholder.typicode.com";
        client.debugMode = true;
        
        // Make a simple GET request
        client.Get("/posts/1", 
            onSuccess: (jsonResponse) => {
                Debug.Log("API Test Success!");
                Debug.Log("Response: " + jsonResponse);
                
                // Try to parse the JSON
                TestPost post = JsonUtility.FromJson<TestPost>(jsonResponse);
                Debug.Log($"Parsed: Title='{post.title}', UserID={post.userId}");
                
                Destroy(tempObject);
            },
            onError: (error) => {
                Debug.LogError("API Test Failed: " + error);
                Destroy(tempObject);
            }
        );
    }
    
    void Update()
    {
        // Debug controls
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Testing simple API call...");
            TestSimpleApiCall();
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Simulating game over with API submission...");
            OnGameOver(1500, 75); // Test with fake score
        }
    }
}

/// <summary>
/// Example data class for testing with JSONPlaceholder API
/// </summary>
[System.Serializable]
public class TestPost
{
    public int userId;
    public int id;
    public string title;
    public string body;
}
