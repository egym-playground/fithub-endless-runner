using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple leaderboard UI that works with a ScrollView and entry prefabs.
/// Shows "No entries" message when leaderboard is empty.
/// </summary>
public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject leaderboardEntryPrefab;  // Prefab with name, score, coins texts
    public Transform scrollContent;            // ScrollView -> Viewport -> Content
    public GameObject noEntriesMessage;        // GameObject to show when no data
    public Text statusText;                    // Optional status text
    
    private GameApiManager apiManager;
    
    void Start()
    {
        SetupApiManager();
        SetupScrollViewLayout();
        
        // Delay loading to ensure GameApiManager is fully initialized
        StartCoroutine(LoadLeaderboardAfterDelay());
    }
    
    System.Collections.IEnumerator LoadLeaderboardAfterDelay()
    {
        // Wait a frame to ensure all Start() methods have been called
        yield return null;
        LoadLeaderboard();
    }
    
    void SetupScrollViewLayout()
    {
        if (scrollContent != null)
        {
            Debug.Log("Setting up ScrollView layout...");
            
            // Remove any existing layout components that might conflict
            VerticalLayoutGroup existingLayout = scrollContent.GetComponent<VerticalLayoutGroup>();
            if (existingLayout != null) 
            {
                DestroyImmediate(existingLayout);
                Debug.Log("Removed existing VerticalLayoutGroup");
            }
            
            ContentSizeFitter existingFitter = scrollContent.GetComponent<ContentSizeFitter>();
            if (existingFitter != null)
            {
                DestroyImmediate(existingFitter);
                Debug.Log("Removed existing ContentSizeFitter");
            }
            
            // Set up the RectTransform for the content
            RectTransform contentRect = scrollContent.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                // Anchor to top and stretch horizontally
                contentRect.anchorMin = new Vector2(0, 1);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.pivot = new Vector2(0.5f, 1);
                contentRect.anchoredPosition = Vector2.zero;
                contentRect.sizeDelta = new Vector2(0, 100); // Start with small height, will grow
            }
            
            Debug.Log("ScrollView configured for manual positioning");
        }
    }
    
    void SetupApiManager()
    {
        // Find or create API manager - REAL SERVER ONLY
        apiManager = FindObjectOfType<GameApiManager>();
        if (apiManager == null)
        {
            GameObject apiObject = new GameObject("GameApiManager");
            apiManager = apiObject.AddComponent<GameApiManager>();
            Debug.Log("Created GameApiManager for real server API");
        }
        else
        {
            Debug.Log("Using existing GameApiManager for real server API");
        }
    }
    
    public void LoadLeaderboard()
    {
        Debug.Log("Loading leaderboard from REAL SERVER...");
        
        // Clear existing entries
        ClearLeaderboard();
        ShowNoEntries(false);
        
        // Use REAL API only - no mock
        if (apiManager != null)
        {
            Debug.Log($"Connecting to real server at: {apiManager.apiBaseUrl}");
            
            // Ensure the API manager is initialized
            apiManager.EnsureInitialized();
            
            apiManager.GetLeaderboard(OnLeaderboardSuccess, OnLeaderboardError);
        }
        else
        {
            OnLeaderboardError("GameApiManager not available - cannot connect to real server");
        }
    }
    
    void OnLeaderboardSuccess(LeaderboardEntry[] entries)
    {
        Debug.Log($"Leaderboard loaded: {entries.Length} entries");
        Debug.Log($"ScrollContent assigned: {scrollContent != null}");
        Debug.Log($"Prefab assigned: {leaderboardEntryPrefab != null}");
        
        if (entries.Length == 0)
        {
            ShowNoEntries(true);
            UpdateStatus("No players yet - be the first!");
            return;
        }
        
        Debug.Log("Starting to create leaderboard entries...");
        
        // Create UI entries
        for (int i = 0; i < entries.Length; i++)
        {
            Debug.Log($"Creating entry {i + 1}/{entries.Length}: {entries[i].playerName}");
            CreateLeaderboardEntry(entries[i]);
        }
        
        Debug.Log($"Finished creating {entries.Length} leaderboard entries");
        Debug.Log($"ScrollContent now has {scrollContent.childCount} children");
        UpdateStatus($"Showing {entries.Length} players");
    }
    
    void OnLeaderboardError(string error)
    {
        Debug.LogError($"Leaderboard error: {error}");
        ShowNoEntries(true, "Failed to load leaderboard");
        UpdateStatus("Connection error");
    }
    
    void CreateLeaderboardEntry(LeaderboardEntry entry)
    {
        if (leaderboardEntryPrefab == null || scrollContent == null)
        {
            Debug.LogError("Leaderboard prefab or scroll content not assigned!");
            return;
        }
        
        Debug.Log($"Creating entry for #{entry.rank} {entry.playerName}");
        
        // Calculate position for this entry
        int entryIndex = scrollContent.childCount;
        float entryHeight = 60f; // Height of each entry
        float spacing = 10f;     // Space between entries
        float yPosition = -(entryIndex * (entryHeight + spacing)); // Negative because we go down
        
        // Instantiate the prefab
        GameObject entryObj = Instantiate(leaderboardEntryPrefab, scrollContent);
        
        // Position the entry manually
        RectTransform entryRect = entryObj.GetComponent<RectTransform>();
        if (entryRect != null)
        {
            // Anchor to top-left and stretch horizontally
            entryRect.anchorMin = new Vector2(0, 1);
            entryRect.anchorMax = new Vector2(1, 1);
            entryRect.pivot = new Vector2(0.5f, 1);
            entryRect.anchoredPosition = new Vector2(0, yPosition);
            entryRect.sizeDelta = new Vector2(0, entryHeight); // Width=0 means stretch, height=fixed
        }
        
        // Update content size to fit all entries
        RectTransform contentRect = scrollContent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            float totalHeight = (entryIndex + 1) * (entryHeight + spacing);
            contentRect.sizeDelta = new Vector2(0, totalHeight);
        }
        
        // Set the data
        LeaderboardEntryUI entryUI = entryObj.GetComponent<LeaderboardEntryUI>();
        if (entryUI != null)
        {
            entryUI.SetData(entry);
        }
        else
        {
            SetEntryData(entryObj, entry);
        }
        
        Debug.Log($"Entry positioned at Y:{yPosition}, #{entry.rank} {entry.playerName}");
    }
    
    void SetEntryData(GameObject entryObj, LeaderboardEntry entry)
    {
        // Find text components (customize names to match your prefab)
        Text nameText = entryObj.transform.Find("NameText")?.GetComponent<Text>();
        Text scoreText = entryObj.transform.Find("ScoreText")?.GetComponent<Text>();
        Text coinsText = entryObj.transform.Find("CoinsText")?.GetComponent<Text>();
        Text rankText = entryObj.transform.Find("RankText")?.GetComponent<Text>();
        
        // Set the data
        if (nameText != null) nameText.text = entry.playerName;
        if (scoreText != null) scoreText.text = entry.score.ToString("N0");
        if (coinsText != null) coinsText.text = entry.coins.ToString();
        if (rankText != null) rankText.text = $"#{entry.rank}";
        
        Debug.Log($"Entry created: #{entry.rank} {entry.playerName} - {entry.score} pts, {entry.coins} coins");
    }
    
    void ShowNoEntries(bool show, string message = "No leaderboard entries yet")
    {
        if (noEntriesMessage != null)
        {
            noEntriesMessage.SetActive(show);
            
            // Update message text if there's a Text component
            Text messageText = noEntriesMessage.GetComponent<Text>();
            if (messageText != null)
            {
                messageText.text = message;
            }
        }
    }
    
    void UpdateStatus(string status)
    {
        if (statusText != null)
        {
            statusText.text = status;
        }
        Debug.Log($"Leaderboard Status: {status}");
    }
    
    void ClearLeaderboard()
    {
        if (scrollContent != null)
        {
            foreach (Transform child in scrollContent)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    void Update()
    {
        // Simple debug controls
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Refreshing leaderboard...");
            LoadLeaderboard();
        }
    }
}
