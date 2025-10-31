# Leaderboard System Setup Guide

## 🎯 Overview

This system automatically fetches leaderboard data every time you enter the leaderboard scene and displays player names, scores, and coins in a clean UI.

## 📁 Files Created

- **`LeaderboardUI.cs`** - Main leaderboard display controller
- **`MockApiManager.cs`** - Mock API for testing without a server
- **`ExampleLeaderboardResponse.json`** - Example of expected API response format
- **Updated `GameApiManager.cs`** - Enhanced API management

## 🚀 Setup Instructions

### 1. Leaderboard Scene Setup

In your **Leaderboard scene**:

1. **Add LeaderboardUI Script**:
   - Create an empty GameObject named "LeaderboardManager"
   - Add the `LeaderboardUI` component

2. **Create UI Elements**:
   ```
   Canvas
   ├── LeaderboardPanel
   │   ├── Title (Text: "Leaderboard")
   │   ├── StatusText (Text: "Loading...")
   │   ├── ScrollView
   │   │   └── Content (Container for entries)
   │   ├── RefreshButton (Button: "Refresh")
   │   └── BackButton (Button: "Back")
   └── LoadingPanel
       └── LoadingText (Text: "Loading leaderboard...")
   ```

3. **Assign References** in LeaderboardUI inspector:
   - `leaderboardContainer` → ScrollView/Content
   - `statusText` → StatusText
   - `refreshButton` → RefreshButton  
   - `backButton` → BackButton
   - `loadingPanel` → LoadingPanel
   - `loadingText` → LoadingText

### 2. API Configuration

**For Testing (Recommended first):**
- The system automatically creates a `MockApiManager` 
- Press **'M'** key to toggle between mock/real API
- Mock API generates fake leaderboard data

**For Production:**
1. Configure `GameApiManager` settings:
   ```csharp
   apiBaseUrl = "https://your-api.com/api"
   leaderboardEndpoint = "/leaderboard"
   ```

2. Your API should return JSON in this format:
   ```json
   {
     "success": true,
     "message": "Leaderboard retrieved successfully",
     "data": [
       {
         "rank": 1,
         "playerName": "ProGamer2024",
         "score": 25847,
         "coins": 432,
         "timestamp": "2025-10-30 14:23:15"
       }
       // ... more entries
     ]
   }
   ```

## 🎮 How It Works

### Automatic Loading
- **Scene Load**: Leaderboard data is fetched automatically when entering the scene
- **Loading State**: Shows "Loading leaderboard..." during fetch
- **Error Handling**: Displays error messages if API fails
- **Animation**: Entries appear one by one with smooth animation

### Display Format
Each leaderboard entry shows:
- **Rank**: #1, #2, #3, etc.
- **Player Name**: Full username  
- **Score**: Formatted with commas (e.g., "25,847")
- **Coins**: Shows coin count (e.g., "432 coins")

### Color Coding
- **🥇 Rank 1**: Gold color
- **🥈 Rank 2**: Silver color  
- **🥉 Rank 3**: Bronze color
- **Others**: White color

## 🔧 Features

### ✅ **Auto-Refresh**: Loads data when scene starts
### ✅ **Manual Refresh**: "Refresh" button to reload data
### ✅ **Loading States**: Visual feedback during loading
### ✅ **Error Handling**: Shows error messages
### ✅ **Responsive UI**: Creates entries dynamically
### ✅ **Mock API**: Test without real server
### ✅ **Debug Controls**: Keyboard shortcuts for testing

## 🎯 Debug Controls

- **Press 'R'**: Manual refresh leaderboard
- **Press 'M'**: Toggle Mock API on/off
- **Press 'L'**: Get leaderboard (from GameApiManager)
- **Press 'Escape'**: Go back to main menu

## 📊 API Response Format

Your server endpoint should return:

```json
{
  "success": true,
  "message": "Leaderboard retrieved successfully", 
  "data": [
    {
      "rank": 1,
      "playerName": "ProGamer2024",
      "score": 25847,
      "coins": 432,
      "timestamp": "2025-10-30 14:23:15"
    },
    {
      "rank": 2,
      "playerName": "SpeedRunner", 
      "score": 22156,
      "coins": 389,
      "timestamp": "2025-10-30 13:45:32"
    }
    // ... up to top 50 players
  ],
  "totalEntries": 247,
  "lastUpdated": "2025-10-30 14:25:00"
}
```

## 🔧 Customization Options

### LeaderboardUI Settings
```csharp
[Header("Settings")]
public int maxEntriesToShow = 10;          // How many to display
public bool autoRefreshOnStart = true;     // Auto-load on scene start  
public float refreshAnimationSpeed = 0.1f; // Animation delay between entries
```

### MockApiManager Settings
```csharp
[Header("Mock API Settings")]  
public bool useMockApi = true;           // Use fake data
public float mockResponseDelay = 1.5f;   // Simulate network delay
```

## 🚀 Quick Testing

1. **Add LeaderboardUI** to any GameObject in your Leaderboard scene
2. **Run the scene** - it will automatically create UI and fetch mock data
3. **Press 'R'** to refresh manually
4. **Press 'M'** to toggle between mock and real API

## 🔌 Integration with Game Over

To submit scores that appear in leaderboard:

```csharp
// In your game over logic
GameApiManager apiManager = FindObjectOfType<GameApiManager>();
if (apiManager != null) 
{
    apiManager.SubmitScore(playerName, finalScore, totalCoins, 
        onComplete: (success) => {
            if (success) Debug.Log("Score submitted to leaderboard!");
        }
    );
}
```

## 📱 UI Requirements

The system works with:
- **Automatic UI Creation**: Creates simple entries if no prefab provided
- **Custom Prefab**: Use your own leaderboard entry prefab
- **Responsive Layout**: Adapts to different screen sizes
- **Scroll Support**: Built-in scrolling for long lists

## 🛠️ Troubleshooting

### No entries showing?
1. Check console for "Leaderboard received: X entries"
2. Verify `leaderboardContainer` is assigned
3. Enable `debugMode` in GameApiManager

### API errors?
1. Use Mock API first (`useMockApi = true`)
2. Check API URL and endpoint paths
3. Verify JSON response format matches expected structure

### UI not appearing?  
1. Ensure LeaderboardUI script is on an active GameObject
2. Check that UI references are assigned in inspector
3. Look for error messages in console

The system is designed to work immediately with mock data, then easily switch to your real API when ready! 🎮
