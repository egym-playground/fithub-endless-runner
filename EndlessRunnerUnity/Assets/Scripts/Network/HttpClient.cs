using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A flexible HTTP client for making requests and parsing JSON responses.
/// Supports GET, POST, PUT, DELETE methods with automatic JSON serialization/deserialization.
/// </summary>
public class HttpClient : MonoBehaviour
{
    [Header("HTTP Client Settings")]
    public string baseUrl = "https://api.example.com";
    public int timeoutSeconds = 30;
    public bool debugMode = true;
    
    [Header("Headers")]
    public string contentType = "application/json";
    public string userAgent = "Unity-EndlessRunner/1.0";
    
    // Events for handling responses
    public static event Action<string> OnRequestSuccess;
    public static event Action<string> OnRequestError;
    
    #region Public Methods
    
    /// <summary>
    /// Make a GET request to the specified endpoint
    /// </summary>
    /// <param name="endpoint">The API endpoint (e.g., "/users/123")</param>
    /// <param name="onSuccess">Callback for successful response</param>
    /// <param name="onError">Callback for error response</param>
    public void Get(string endpoint, Action<string> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(SendRequest("GET", endpoint, null, onSuccess, onError));
    }
    
    /// <summary>
    /// Make a GET request and automatically parse JSON to specified type
    /// </summary>
    public void Get<T>(string endpoint, Action<T> onSuccess = null, Action<string> onError = null)
    {
        Get(endpoint, 
            (jsonResponse) => {
                try 
                {
                    T data = JsonUtility.FromJson<T>(jsonResponse);
                    onSuccess?.Invoke(data);
                }
                catch (Exception e)
                {
                    LogError($"JSON Parsing Error: {e.Message}");
                    onError?.Invoke($"Failed to parse JSON: {e.Message}");
                }
            }, 
            onError);
    }
    
    /// <summary>
    /// Make a POST request with JSON data
    /// </summary>
    /// <param name="endpoint">The API endpoint</param>
    /// <param name="data">Object to serialize as JSON</param>
    /// <param name="onSuccess">Callback for successful response</param>
    /// <param name="onError">Callback for error response</param>
    public void Post<T>(string endpoint, T data, Action<string> onSuccess = null, Action<string> onError = null)
    {
        string jsonData = JsonUtility.ToJson(data);
        StartCoroutine(SendRequest("POST", endpoint, jsonData, onSuccess, onError));
    }
    
    /// <summary>
    /// Make a POST request and automatically parse JSON response
    /// </summary>
    public void Post<TRequest, TResponse>(string endpoint, TRequest data, Action<TResponse> onSuccess = null, Action<string> onError = null)
    {
        string jsonData = JsonUtility.ToJson(data);
        StartCoroutine(SendRequest("POST", endpoint, jsonData, 
            (jsonResponse) => {
                try 
                {
                    TResponse responseData = JsonUtility.FromJson<TResponse>(jsonResponse);
                    onSuccess?.Invoke(responseData);
                }
                catch (Exception e)
                {
                    LogError($"JSON Parsing Error: {e.Message}");
                    onError?.Invoke($"Failed to parse JSON: {e.Message}");
                }
            }, 
            onError));
    }
    
    /// <summary>
    /// Make a PUT request with JSON data
    /// </summary>
    public void Put<T>(string endpoint, T data, Action<string> onSuccess = null, Action<string> onError = null)
    {
        string jsonData = JsonUtility.ToJson(data);
        StartCoroutine(SendRequest("PUT", endpoint, jsonData, onSuccess, onError));
    }
    
    /// <summary>
    /// Make a DELETE request
    /// </summary>
    public void Delete(string endpoint, Action<string> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(SendRequest("DELETE", endpoint, null, onSuccess, onError));
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Core method for sending HTTP requests
    /// </summary>
    private IEnumerator SendRequest(string method, string endpoint, string jsonData, Action<string> onSuccess, Action<string> onError)
    {
        string fullUrl = baseUrl + endpoint;
        
        Log($"Sending {method} request to: {fullUrl}");
        
        using (UnityWebRequest request = new UnityWebRequest(fullUrl, method))
        {
            // Set timeout
            request.timeout = timeoutSeconds;
            
            // Add headers
            request.SetRequestHeader("Content-Type", contentType);
            request.SetRequestHeader("User-Agent", userAgent);
            
            // Add request body for POST/PUT
            if (!string.IsNullOrEmpty(jsonData))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                Log($"Request Body: {jsonData}");
            }
            
            // Set download handler
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // Send request
            yield return request.SendWebRequest();
            
            // Handle response
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Log($"Request successful! Response: {responseText}");
                
                onSuccess?.Invoke(responseText);
                OnRequestSuccess?.Invoke(responseText);
            }
            else
            {
                string errorMessage = $"Request failed: {request.error} (Code: {request.responseCode})";
                
                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    errorMessage += $"\nResponse: {request.downloadHandler.text}";
                }
                
                LogError(errorMessage);
                onError?.Invoke(errorMessage);
                OnRequestError?.Invoke(errorMessage);
            }
        }
    }
    
    private void Log(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[HttpClient] {message}");
        }
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[HttpClient] {message}");
    }
    
    #endregion
}

#region Example Data Classes

/// <summary>
/// Example data class for API responses - customize these for your specific API
/// </summary>
[Serializable]
public class ApiResponse<T>
{
    public bool success;
    public string message;
    public T data;
}

/// <summary>
/// Example user data class
/// </summary>
[Serializable]
public class UserData
{
    public int id;
    public string username;
    public string email;
    public int score;
    public DateTime createdAt;
}

/// <summary>
/// Example leaderboard entry
/// </summary>
[Serializable]
public class LeaderboardEntry
{
    public int rank;
    public string playerName;
    public int score;
    public int coins;
}

/// <summary>
/// Example game stats
/// </summary>
[Serializable]
public class GameStats
{
    public int totalGames;
    public int highScore;
    public int totalCoins;
    public float averageScore;
    public string lastPlayed;
}

#endregion
