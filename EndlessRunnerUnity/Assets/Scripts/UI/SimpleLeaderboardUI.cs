using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple text-based leaderboard that creates one text line per entry.
/// No prefabs needed - creates simple text entries automatically.
/// </summary>
public class SimpleLeaderboardUI : MonoBehaviour
{
    [Header("Simple Setup")]
    public Transform scrollContent;        // ScrollView -> Viewport -> Content
    public GameObject noEntriesMessage;    // Text to show when empty
    
    [Header("Entry Settings")]
    public Font textFont;                 // Font to use (optional)
    public int fontSize = 24;             // Size of text
    public Color textColor = Color.white; // Color of text
    public float entryHeight = 40f;       // Height of each entry
    public float entrySpacing = 5f;       // Space between entries
    
    private GameApiManager apiManager;
    
    void Start()
    {
        SetupApiManager();
        SetupScrollView();
        LoadLeaderboard();
    }
    
    void SetupApiManager()
    {
        apiManager = FindObjectOfType<GameApiManager>();
        if (apiManager == null)
        {
            GameObject apiObject = new GameObject("GameApiManager");
            apiManager = apiObject.AddComponent<GameApiManager>();
            apiObject.AddComponent<MockApiManager>();
        }
    }
    
    void SetupScrollView()
    {
        if (scrollContent != null)
        {
            RectTransform contentRect = scrollContent.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            
            Debug.Log("Simple ScrollView setup complete");
        }
    }
    
    public void LoadLeaderboard()
    {
        Debug.Log("Loading simple leaderboard...");
        ClearLeaderboard();
        
        MockApiManager mockApi = FindObjectOfType<MockApiManager>();
        if (mockApi != null && mockApi.useMockApi)
        {
            mockApi.GetMockLeaderboard(OnLeaderboardSuccess, OnLeaderboardError);
        }
        else if (apiManager != null)
        {
            apiManager.GetLeaderboard(OnLeaderboardSuccess, OnLeaderboardError);
        }
    }
    
    void OnLeaderboardSuccess(LeaderboardEntry[] entries)
    {
        Debug.Log($"Simple leaderboard: {entries.Length} entries");
        
        if (entries.Length == 0)
        {
            ShowNoEntries(true);
            return;
        }
        
        ShowNoEntries(false);
        
        for (int i = 0; i < entries.Length; i++)
        {
            CreateSimpleEntry(entries[i], i);
        }
        
        // Resize content to fit all entries
        UpdateContentSize(entries.Length);
    }
    
    void CreateSimpleEntry(LeaderboardEntry entry, int index)
    {
        // Create a simple text GameObject
        GameObject entryObj = new GameObject($"Entry_{entry.rank}");
        entryObj.transform.SetParent(scrollContent, false);
        
        // Add Text component
        Text textComp = entryObj.AddComponent<Text>();
        textComp.text = $"#{entry.rank}  {entry.playerName}  -  Score: {entry.score:N0}  -  Coins: {entry.coins}";
        textComp.font = textFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.fontSize = fontSize;
        textComp.color = textColor;
        textComp.alignment = TextAnchor.MiddleLeft;
        
        // Special colors for top 3
        if (entry.rank == 1) textComp.color = Color.yellow;      // Gold
        else if (entry.rank == 2) textComp.color = Color.gray;   // Silver  
        else if (entry.rank == 3) textComp.color = new Color(0.8f, 0.5f, 0.2f); // Bronze
        
        // Position the entry
        RectTransform rect = entryObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0, 1);
        
        float yPos = -(index * (entryHeight + entrySpacing));
        rect.anchoredPosition = new Vector2(10, yPos); // 10px left padding
        rect.sizeDelta = new Vector2(-20, entryHeight); // -20 for left+right padding
        
        Debug.Log($"Created simple entry: {textComp.text}");
    }
    
    void UpdateContentSize(int entryCount)
    {
        if (scrollContent != null)
        {
            RectTransform contentRect = scrollContent.GetComponent<RectTransform>();
            float totalHeight = entryCount * (entryHeight + entrySpacing) + 20; // +20 for padding
            contentRect.sizeDelta = new Vector2(0, totalHeight);
            
            Debug.Log($"Content resized to height: {totalHeight} for {entryCount} entries");
        }
    }
    
    void OnLeaderboardError(string error)
    {
        Debug.LogError($"Simple leaderboard error: {error}");
        ShowNoEntries(true, "Failed to load leaderboard");
    }
    
    void ShowNoEntries(bool show, string message = "No entries yet")
    {
        if (noEntriesMessage != null)
        {
            noEntriesMessage.SetActive(show);
            Text messageText = noEntriesMessage.GetComponent<Text>();
            if (messageText != null) messageText.text = message;
        }
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            LoadLeaderboard();
        }
    }
}
