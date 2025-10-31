# Simple Leaderboard Setup Guide

## 🎯 **Simplified Setup**

### 1. **ScrollView Setup**
Create this hierarchy in your Leaderboard scene:
```
Canvas
├── ScrollView
│   └── Viewport  
│       └── Content (this is your scrollContent)
├── NoEntriesMessage (Text: "No players yet - be the first!")
└── StatusText (Text: "Loading...")
```

### 2. **Leaderboard Entry Prefab**
Create a prefab with this structure:
```
LeaderboardEntry (with LeaderboardEntryUI component)
├── RankText (Text: "#1")
├── NameText (Text: "PlayerName") 
├── ScoreText (Text: "12345")
└── CoinsText (Text: "567")
```

### 3. **Assign References**
In LeaderboardUI component:
- `leaderboardEntryPrefab` → Your entry prefab
- `scrollContent` → ScrollView/Viewport/Content
- `noEntriesMessage` → NoEntriesMessage GameObject
- `statusText` → StatusText (optional)

## 🚀 **How It Works**

1. **Auto-loads** leaderboard data when scene starts
2. **Creates entries** by instantiating your prefab for each player
3. **Shows "No entries"** message when leaderboard is empty
4. **Handles errors** gracefully with fallback messages

## 🎮 **Entry Data Display**

Each entry shows:
- **Rank**: #1, #2, #3...
- **Name**: Player name
- **Score**: Formatted with commas (25,847)
- **Coins**: Raw number (432)

## 🎨 **Automatic Styling**

- **🥇 Rank 1**: Gold color
- **🥈 Rank 2**: Silver color
- **🥉 Rank 3**: Bronze color
- **Others**: White color

## 🔧 **Debug Controls**

- **Press 'R'**: Refresh leaderboard
- **Press 'M'**: Toggle Mock API (in MockApiManager)

## 📝 **Quick Setup Steps**

1. Add `LeaderboardUI` script to any GameObject
2. Create ScrollView with Content transform
3. Create entry prefab with `LeaderboardEntryUI` component
4. Assign references in inspector
5. Done! Auto-loads on scene start

## 🛠️ **Entry Prefab Setup**

1. Create empty GameObject named "LeaderboardEntry"
2. Add `LeaderboardEntryUI` component
3. Add Text components for rank, name, score, coins
4. Assign Text references in LeaderboardEntryUI
5. Save as prefab

## 📱 **Empty State Handling**

When no entries exist:
- Hides ScrollView entries
- Shows `noEntriesMessage` GameObject
- Updates status to "No players yet - be the first!"

Perfect for a clean, simple leaderboard that just works! 🎯
