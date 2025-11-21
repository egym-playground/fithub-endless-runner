using System;
using UnityEngine;

/// <summary>
/// Game-specific API manager for the Endless Runner game.
/// Handles leaderboard, user stats, and game data synchronization.
/// </summary>
public class GameApiManager : MonoBehaviour
{
    [Header("API Configuration")]
    public string apiBaseUrl = "http://127.0.0.1:5000";
    
    [Header("Endpoints")]
    public string leaderboardEndpoint = "/score";
    public string submitScoreEndpoint = "/score";
    
    private HttpClient httpClient;
    
    void Awake()
    {
        // Initialize earlier in Awake to ensure it's ready before other components
        SetupHttpClient();
    }
    
    void Start()
    {
        // Ensure initialization happened
        if (httpClient == null)
        {
            Debug.LogWarning("HttpClient was null in Start, re-initializing...");
            SetupHttpClient();
        }
        
        // Don't automatically load leaderboard - let specific UI handle it
        Debug.Log("GameApiManager ready for API requests");
    }
    
    /// <summary>
    /// Public method to ensure initialization - call this before using the API
    /// </summary>
    public void EnsureInitialized()
    {
        if (httpClient == null)
        {
            SetupHttpClient();
        }
    }
    
    void SetupHttpClient()
    {
        // Add HttpClient component if it doesn't exist
        httpClient = GetComponent<HttpClient>();
        if (httpClient == null)
        {
            httpClient = gameObject.AddComponent<HttpClient>();
        }
        
        // Configure the HTTP client
        httpClient.baseUrl = apiBaseUrl;
        httpClient.timeoutSeconds = 15;
        httpClient.debugMode = true;
        
        Debug.Log("GameApiManager initialized with base URL: " + apiBaseUrl);
    }
    
    #region Public API Methods
    
    /// <summary>
    /// Get the current leaderboard
    /// </summary>
    public void GetLeaderboard(Action<LeaderboardEntry[]> onSuccess = null, Action<string> onError = null)
    {
        // Ensure HttpClient is initialized
        if (httpClient == null)
        {
            Debug.LogWarning("HttpClient not initialized, setting up now...");
            SetupHttpClient();
        }
        
        if (httpClient == null)
        {
            Debug.LogError("Failed to initialize HttpClient!");
            onError?.Invoke("HttpClient initialization failed");
            return;
        }
        
        Debug.Log("Fetching leaderboard from: " + apiBaseUrl + leaderboardEndpoint);
        
        // First get raw response to see what we're receiving
        httpClient.Get(
            leaderboardEndpoint,
            (rawResponse) => {
                Debug.Log("=== RAW LEADERBOARD RESPONSE ===");
                Debug.Log(rawResponse);
                Debug.Log("==============================");
                
                // Now try to parse the response - handle your server's actual format
                try 
                {
                    // Your server returns: {"data": [...]} format
                    ServerLeaderboardResponse serverResponse = JsonUtility.FromJson<ServerLeaderboardResponse>(rawResponse);
                    
                    if (serverResponse != null && serverResponse.data != null)
                    {
                        Debug.Log($"Leaderboard parsed successfully: {serverResponse.data.Length} entries");
                        onSuccess?.Invoke(serverResponse.data);
                        
                        // Display top 5 in console for debugging
                        Debug.Log("=== TOP LEADERBOARD ENTRIES ===");
                        for (int i = 0; i < Mathf.Min(5, serverResponse.data.Length); i++)
                        {
                            var entry = serverResponse.data[i];
                            Debug.Log($"#{entry.rank}: {entry.playerName} - Score: {entry.score:N0} - Coins: {entry.coins}");
                        }
                        Debug.Log("==============================");
                    }
                    else
                    {
                        Debug.LogWarning("Server response is missing data field");
                        onError?.Invoke("Server response is missing data field");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse leaderboard JSON: {e.Message}");
                    onError?.Invoke($"JSON parsing error: {e.Message}");
                }
            },
            (error) => {
                Debug.LogError($"Failed to get leaderboard: {error}");
                onError?.Invoke(error);
            }
        );
    }
    
    /// <summary>
    /// Submit a new score to the server
    /// </summary>
    public void SubmitScore(string playerName, int score, int coins, Action<bool> onComplete = null)
    {
        // Ensure HttpClient is initialized
        if (httpClient == null)
        {
            Debug.LogWarning("HttpClient not initialized, setting up now...");
            SetupHttpClient();
        }
        
        if (httpClient == null)
        {
            Debug.LogError("Failed to initialize HttpClient!");
            onComplete?.Invoke(false);
            return;
        }
        
        var scoreData = new ScoreSubmission
        {
            playerName = playerName,
            score = score,
            coins = coins,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        };
        
        Debug.Log($"Submitting score for {playerName}: {score} points, {coins} coins");
        
        httpClient.Post<ScoreSubmission, ApiResponse<object>>(
            submitScoreEndpoint,
            scoreData,
            (response) => {
                if (response.success)
                {
                    Debug.Log("Score submitted successfully!");
                    onComplete?.Invoke(true);
                    
                    // Refresh leaderboard after successful submission
                    GetLeaderboard();
                }
                else
                {
                    Debug.LogWarning($"Score submission failed: {response.message}");
                    onComplete?.Invoke(false);
                }
            },
            (error) => {
                Debug.LogError($"Failed to submit score: {error}");
                onComplete?.Invoke(false);
            }
        );
    }
    
    /// <summary>
    /// Simple example: Just get raw JSON data from any endpoint
    /// </summary>
    public void GetRawData(string endpoint, Action<string> onSuccess = null)
    {
        httpClient.Get(endpoint, onSuccess, (error) => {
            Debug.LogError($"Raw data request failed: {error}");
        });
    }
    
    /// <summary>
    /// Submit game over score when player loses
    /// </summary>
    public void SubmitGameOverScore(GameOverScoreData scoreData, System.Action<bool> onComplete = null)
    {
        Debug.Log("🎯 === GameApiManager.SubmitGameOverScore STARTED ===");
        
        // Validate endpoint
        if (string.IsNullOrEmpty(submitScoreEndpoint))
        {
            Debug.LogError("❌ Game over score endpoint not configured!");
            Debug.LogError("Please set the submitScoreEndpoint in GameApiManager inspector or code");
            onComplete?.Invoke(false);
            return;
        }
        
        // Validate HttpClient
        if (httpClient == null)
        {
            Debug.LogError("❌ HttpClient is null! Cannot submit game over score.");
            onComplete?.Invoke(false);
            return;
        }
        
        Debug.Log($"📊 Score Data to Submit: Score={scoreData.score}, Coins={scoreData.coins}");
        Debug.Log($"🌐 Target Endpoint: {apiBaseUrl}{submitScoreEndpoint}");
        Debug.Log($"📝 JSON Payload: {JsonUtility.ToJson(scoreData)}");
        
        Debug.Log("🚀 Sending POST request...");
        
        httpClient.Post<GameOverScoreData, ApiResponse<object>>(
            submitScoreEndpoint,
            scoreData,
            (response) => {
                Debug.Log("📥 === HTTP RESPONSE RECEIVED ===");
                if (response != null && response.success)
                {
                    Debug.Log("✅ Game over score submitted successfully!");
                    Debug.Log($"✨ Response: {JsonUtility.ToJson(response)}");
                    onComplete?.Invoke(true);
                }
                else
                {
                    string errorMsg = response?.message ?? "Unknown error occurred";
                    Debug.LogWarning($"⚠️ Game over score submission failed: {errorMsg}");
                    if (response != null)
                    {
                        Debug.LogWarning($"📋 Full Response: {JsonUtility.ToJson(response)}");
                    }
                    onComplete?.Invoke(false);
                }
            },
            (error) => {
                Debug.LogError("💥 === HTTP REQUEST FAILED ===");
                Debug.LogError($"❌ Failed to submit game over score: {error}");
                Debug.LogError($"🔗 Attempted URL: {apiBaseUrl}{submitScoreEndpoint}");
                onComplete?.Invoke(false);
            }
        );
    }
    
    #endregion
    
    #region Integration with Game
    
    /// <summary>
    /// Call this when the game ends to submit score
    /// </summary>
    public void OnGameEnd(int finalScore, int totalCoins)
    {
        string playerName = GetPlayerName();
        SubmitScore(playerName, finalScore, totalCoins);
    }
    
    /// <summary>
    /// Get player name from PlayerPrefs or generate a default one
    /// </summary>
    private string GetPlayerName()
    {
        string savedName = PlayerPrefs.GetString("PlayerName", "");
        if (string.IsNullOrEmpty(savedName))
        {
            savedName = "Player" + UnityEngine.Random.Range(1000, 9999);
            PlayerPrefs.SetString("PlayerName", savedName);
            PlayerPrefs.Save();
        }
        return savedName;
    }
    
    #endregion
    
    #region Testing Methods
    
    void Update()
    {
        // Debug keys for testing
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Testing: Get Leaderboard");
            GetLeaderboard();
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Testing: Submit Random Score");
            int randomScore = UnityEngine.Random.Range(100, 1000);
            int randomCoins = UnityEngine.Random.Range(10, 50);
            SubmitScore(GetPlayerName(), randomScore, randomCoins);
        }
        
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("Testing: Get User Stats");
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Testing: Submit Game Over Score");
            var testScoreData = new GameOverScoreData { score = 1500, coins = 75 };
            SubmitGameOverScore(testScoreData);
        }
    }
    
    #endregion
}

#region Data Classes for Game API

/// <summary>
/// Response format from your actual server - matches {"data": [...]} format
/// </summary>
[Serializable]
public class ServerLeaderboardResponse
{
    public LeaderboardEntry[] data;
}

/// <summary>
/// Data structure for submitting scores
/// </summary>
[Serializable]
public class ScoreSubmission
{
    public string playerName;
    public int score;
    public int coins;
    public string timestamp;
}

/// <summary>
/// Data class for game over score submission
/// </summary>
[Serializable]
public class GameOverScoreData
{
    public int score;
    public int coins;
}

/// <summary>
/// Example response for a simple API endpoint
/// </summary>
[Serializable]
public class SimpleApiResponse
{
    public string status;
    public string message;
    public int code;
}

#endregion
