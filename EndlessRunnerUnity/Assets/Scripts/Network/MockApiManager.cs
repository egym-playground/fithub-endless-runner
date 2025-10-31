using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Mock API for testing leaderboard functionality without a real server.
/// This simulates API responses with fake data for development and testing.
/// </summary>
public class MockApiManager : MonoBehaviour
{
    [Header("Mock API Settings")]
    public bool useMockApi = true;
    public float mockResponseDelay = 1.5f;
    
    private GameApiManager realApiManager;
    
    void Start()
    {
        realApiManager = GetComponent<GameApiManager>();
        
        if (useMockApi)
        {
            Debug.Log("Mock API enabled - using fake leaderboard data");
        }
    }
    
    /// <summary>
    /// Get mock leaderboard data
    /// </summary>
    public void GetMockLeaderboard(Action<LeaderboardEntry[]> onSuccess, Action<string> onError = null)
    {
        if (!useMockApi && realApiManager != null)
        {
            // Use real API
            realApiManager.GetLeaderboard(onSuccess, onError);
            return;
        }
        
        StartCoroutine(SimulateLeaderboardRequest(onSuccess, onError));
    }
    
    IEnumerator SimulateLeaderboardRequest(Action<LeaderboardEntry[]> onSuccess, Action<string> onError)
    {
        Debug.Log("Simulating leaderboard API request...");
        
        // Simulate network delay
        yield return new WaitForSeconds(mockResponseDelay);
        
        // 90% chance of success, 10% chance of error for testing
        if (UnityEngine.Random.value < 0.9f)
        {
            LeaderboardEntry[] mockData = GenerateMockLeaderboardData();
            Debug.Log($"Mock API: Returning {mockData.Length} leaderboard entries");
            onSuccess?.Invoke(mockData);
        }
        else
        {
            string error = "Mock API Error: Server temporarily unavailable";
            Debug.LogWarning(error);
            onError?.Invoke(error);
        }
    }
    
    LeaderboardEntry[] GenerateMockLeaderboardData()
    {
        string[] playerNames = {
            "ProGamer2024", "SpeedRunner", "CoinCollector", "JumpMaster", "RunnerX",
            "EndlessRun", "FastFeet", "NinjaRunner", "ScoreChaser", "Player" + UnityEngine.Random.Range(1000, 9999),
            "UltimatePlayer", "GameWinner", "TopRunner", "CoinHunter", "HighScorer"
        };
        
        LeaderboardEntry[] entries = new LeaderboardEntry[playerNames.Length];
        
        // Generate scores in descending order
        int baseScore = 30000;
        
        for (int i = 0; i < entries.Length; i++)
        {
            entries[i] = new LeaderboardEntry
            {
                rank = i + 1,
                playerName = playerNames[i],
                score = baseScore - (i * UnityEngine.Random.Range(800, 1500)),
                coins = UnityEngine.Random.Range(50, 600),
            };
            
            // Ensure scores decrease
            baseScore = entries[i].score - UnityEngine.Random.Range(100, 500);
        }
        
        return entries;
    }
    
    /// <summary>
    /// Submit mock score
    /// </summary>
    public void SubmitMockScore(string playerName, int score, int coins, Action<bool> onComplete)
    {
        if (!useMockApi && realApiManager != null)
        {
            realApiManager.SubmitScore(playerName, score, coins, onComplete);
            return;
        }
        
        StartCoroutine(SimulateScoreSubmission(playerName, score, coins, onComplete));
    }
    
    IEnumerator SimulateScoreSubmission(string playerName, int score, int coins, Action<bool> onComplete)
    {
        Debug.Log($"Mock API: Submitting score for {playerName} - Score: {score}, Coins: {coins}");
        
        yield return new WaitForSeconds(mockResponseDelay * 0.5f);
        
        // 95% success rate for score submission
        bool success = UnityEngine.Random.value < 0.95f;
        
        if (success)
        {
            Debug.Log("Mock API: Score submitted successfully!");
        }
        else
        {
            Debug.LogWarning("Mock API: Score submission failed!");
        }
        
        onComplete?.Invoke(success);
    }
    
    void Update()
    {
        // Toggle mock API with M key
        if (Input.GetKeyDown(KeyCode.M))
        {
            useMockApi = !useMockApi;
            Debug.Log($"Mock API toggled: {(useMockApi ? "ON" : "OFF")}");
        }
    }
}

// Note: To use MockApiManager with LeaderboardUI, 
// modify the LeaderboardUI.RefreshLeaderboard() method to check for MockApiManager first
