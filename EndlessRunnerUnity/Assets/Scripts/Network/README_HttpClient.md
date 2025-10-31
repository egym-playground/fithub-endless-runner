# HTTP Client & API Integration System

A complete HTTP client system for making API requests, parsing JSON responses, and integrating with your Unity game.

## 📁 Files Created

- **`HttpClient.cs`** - Core HTTP client with JSON support
- **`GameApiManager.cs`** - Game-specific API wrapper 
- **`GameApiIntegration.cs`** - Integration with game systems

## 🚀 Quick Start

### 1. Simple API Request
```csharp
// Add HttpClient to any GameObject
HttpClient client = gameObject.AddComponent<HttpClient>();
client.baseUrl = "https://api.example.com";

// Make a GET request
client.Get("/endpoint", 
    onSuccess: (jsonResponse) => {
        Debug.Log("Response: " + jsonResponse);
    },
    onError: (error) => {
        Debug.LogError("Error: " + error);
    }
);
```

### 2. Automatic JSON Parsing
```csharp
// Define your data class
[System.Serializable]
public class UserData 
{
    public int id;
    public string name;
    public int score;
}

// Make request with automatic parsing
client.Get<UserData>("/user/123", 
    onSuccess: (userData) => {
        Debug.Log($"User: {userData.name}, Score: {userData.score}");
    }
);
```

### 3. POST with JSON Data
```csharp
// Send data to server
var scoreData = new { playerName = "John", score = 1500 };

client.Post("/submit-score", scoreData,
    onSuccess: (response) => Debug.Log("Score submitted!"),
    onError: (error) => Debug.LogError(error)
);
```

## 🎮 Game Integration

### Setup for Endless Runner

1. **Add GameApiManager** to a GameObject in your scene
2. **Configure the API settings** in the inspector:
   ```csharp
   apiBaseUrl = "https://your-game-api.com/api"
   apiKey = "your-secret-key"
   ```
3. **Call API methods** from your game events

### Game Over Integration

In your game over logic, submit the score:
```csharp
GameApiManager apiManager = FindObjectOfType<GameApiManager>();
apiManager.OnGameEnd(finalScore, totalCoins);
```

## 🔧 HttpClient Features

### Supported HTTP Methods
- **GET** - Retrieve data
- **POST** - Send data  
- **PUT** - Update data
- **DELETE** - Remove data

### Built-in Features
- ✅ **Automatic JSON serialization/deserialization**
- ✅ **Configurable timeouts**
- ✅ **Custom headers support**
- ✅ **Error handling with callbacks**
- ✅ **Debug logging**
- ✅ **Unity coroutine-based (non-blocking)**

### Example API Endpoints

```csharp
// GET request with automatic JSON parsing
httpClient.Get<LeaderboardEntry[]>("/leaderboard", 
    onSuccess: (entries) => {
        foreach(var entry in entries) {
            Debug.Log($"{entry.rank}. {entry.playerName}: {entry.score}");
        }
    }
);

// POST request with response parsing  
httpClient.Post<ScoreData, ApiResponse>("/submit", scoreData,
    onSuccess: (response) => {
        if(response.success) {
            Debug.Log("Score submitted successfully!");
        }
    }
);
```

## 🎯 Data Classes

Define your API data structures:

```csharp
[System.Serializable]
public class ApiResponse<T>
{
    public bool success;
    public string message;
    public T data;
}

[System.Serializable] 
public class LeaderboardEntry
{
    public int rank;
    public string playerName;
    public int score;
    public int coins;
    public string timestamp;
}

[System.Serializable]
public class ScoreSubmission  
{
    public string playerName;
    public int score;
    public int coins;
    public string apiKey;
}
```

## 🧪 Testing

### Debug Controls (built-in)
- **Press 'L'** - Get Leaderboard
- **Press 'S'** - Submit Random Score  
- **Press 'U'** - Get User Stats
- **Press 'T'** - Test Simple API Call
- **Press 'G'** - Simulate Game Over

### Test with Public API
The system includes a test method using JSONPlaceholder:
```csharp
GameApiIntegration integration = FindObjectOfType<GameApiIntegration>();
integration.TestSimpleApiCall(); // Tests with public API
```

## ⚙️ Configuration

### HttpClient Settings
```csharp
public class HttpClient : MonoBehaviour
{
    public string baseUrl = "https://api.example.com";
    public int timeoutSeconds = 30;
    public bool debugMode = true;
    public string contentType = "application/json";
}
```

### GameApiManager Settings
```csharp
public class GameApiManager : MonoBehaviour  
{
    public string apiBaseUrl = "https://your-game-api.com/api";
    public string apiKey = "your-api-key-here";
    public string leaderboardEndpoint = "/leaderboard";
    public string submitScoreEndpoint = "/score/submit";
}
```

## 🔌 Usage Examples

### 1. Get Leaderboard
```csharp
GameApiManager api = FindObjectOfType<GameApiManager>();
api.GetLeaderboard(
    onSuccess: (entries) => {
        // Update UI with leaderboard data
        UpdateLeaderboardUI(entries);
    },
    onError: (error) => {
        // Show "Offline Mode" or cached data
        ShowOfflineMessage();
    }
);
```

### 2. Submit High Score
```csharp
api.SubmitScore("PlayerName", 1500, 75, 
    onComplete: (success) => {
        if(success) {
            ShowMessage("Score submitted!");
        } else {
            ShowMessage("Failed to submit score");
        }
    }
);
```

### 3. Custom API Call
```csharp
HttpClient client = GetComponent<HttpClient>();
client.baseUrl = "https://your-custom-api.com";

client.Get("/custom-endpoint",
    onSuccess: (json) => {
        // Handle raw JSON response
        Debug.Log("Custom API Response: " + json);
    }
);
```

## 🛠️ Setup Checklist

1. ✅ Add `HttpClient.cs` to your project
2. ✅ Add `GameApiManager.cs` for game-specific logic  
3. ✅ Add `GameApiIntegration.cs` to connect with your game
4. ✅ Create data classes matching your API structure
5. ✅ Configure API endpoints and credentials
6. ✅ Test with debug keys or public API
7. ✅ Integrate with game over system

## 🌐 Example Server Setup

If you need a simple backend, you can use:
- **Node.js + Express** for REST API
- **Firebase** for real-time database
- **Supabase** for PostgreSQL + REST API  
- **JSONBin.io** for simple JSON storage

The HTTP client works with any REST API that returns JSON!

## 🐛 Troubleshooting

### Common Issues

1. **"UnityWebRequest failed"**
   - Check internet connection
   - Verify API URL is correct
   - Check API server is running

2. **"JSON Parsing Error"**
   - Ensure data class fields match API response
   - Use `[System.Serializable]` on data classes
   - Check JSON format in debug logs

3. **"Request timeout"** 
   - Increase `timeoutSeconds` value
   - Check API server response time

### Debug Logging
Enable debug mode to see detailed request/response logs:
```csharp
httpClient.debugMode = true;
```

This will show all HTTP requests, responses, and errors in the Unity console.
