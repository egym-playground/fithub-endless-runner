using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple component for leaderboard entry prefabs.
/// Attach this to your leaderboard entry prefab and assign the text fields.
/// </summary>
public class LeaderboardEntryUI : MonoBehaviour
{
    [Header("Text References")]
    public Text rankText;      // Shows "#1", "#2", etc.
    public Text nameText;      // Shows player name
    public Text scoreText;     // Shows score points
    public Text coinsText;     // Shows coin count
    
    [Header("Optional Styling")]
    public Image background;   // Background image to color
    
    /// <summary>
    /// Set the data for this leaderboard entry
    /// </summary>
    public void SetData(LeaderboardEntry entry)
    {
        // Update text fields
        if (rankText != null) 
            rankText.text = $"#{entry.rank}";
        
        if (nameText != null) 
            nameText.text = entry.playerName;
        
        if (scoreText != null) 
            scoreText.text = entry.score.ToString("N0"); // Format with commas
        
        if (coinsText != null) 
            coinsText.text = entry.coins.ToString();
        
        // Apply special colors for top 3
        ApplyRankStyling(entry.rank);
        
        Debug.Log($"LeaderboardEntry set: #{entry.rank} {entry.playerName} - {entry.score} pts");
    }
    
    void ApplyRankStyling(int rank)
    {
        Color rankColor = Color.white;
        Color bgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Default dark background
        
        // Special colors for top 3
        switch (rank)
        {
            case 1: // Gold
                rankColor = new Color(1f, 0.84f, 0f);
                bgColor = new Color(0.3f, 0.25f, 0f, 0.8f);
                break;
            case 2: // Silver  
                rankColor = new Color(0.75f, 0.75f, 0.75f);
                bgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                break;
            case 3: // Bronze
                rankColor = new Color(0.8f, 0.5f, 0.2f);
                bgColor = new Color(0.25f, 0.15f, 0.05f, 0.8f);
                break;
        }
        
        // Apply rank color to rank text
        if (rankText != null)
            rankText.color = rankColor;
        
        // Apply background color
        if (background != null)
            background.color = bgColor;
    }
}
