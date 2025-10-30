using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using UnityEngine.SceneManagement;

public class SimpleWebSocketManager : MonoBehaviour
{
    [Header("WebSocket Settings")]
    public string serverUrl = "ws://localhost:8080";
    public bool autoConnect = true;
    public float reconnectDelay = 5f;

    [Header("Debug")]
    public bool enableLogging = true;

    [Header("Manual Testing")]
    [Space]
    [Header("Test Controls (Inspector Only)")]
    public bool testLeft = false;
    public bool testRight = false;
    public bool testJump = false;
    public bool testSlide = false;

    private Player player;
    private WebSocket webSocket;
    private bool shouldReconnect = true;

    async void Start()
    {
        player = FindObjectOfType<Player>();
        if (player == null)
        {
            LogError("Player component not found!");
            return;
        }

        Log("SimpleWebSocketManager initialized.");

        if (autoConnect)
        {
            await ConnectToServer();
        }
    }

    void Update()
    {
        // Handle inspector test buttons
        if (testLeft)
        {
            testLeft = false;
            ProcessCommand("left");
        }
        if (testRight)
        {
            testRight = false;
            ProcessCommand("right");
        }
        if (testJump)
        {
            testJump = false;
            ProcessCommand("jump");
        }
        if (testSlide)
        {
            testSlide = false;
            ProcessCommand("slide");
        }

        // Process WebSocket messages
        if (webSocket != null)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            webSocket.DispatchMessageQueue();
#endif
        }

        // Also listen for keyboard input for testing
        if (Input.GetKeyDown(KeyCode.Keypad4)) ProcessCommand("left");
        if (Input.GetKeyDown(KeyCode.Keypad6)) ProcessCommand("right");
        if (Input.GetKeyDown(KeyCode.Keypad8)) ProcessCommand("jump");
        if (Input.GetKeyDown(KeyCode.Keypad2)) ProcessCommand("slide");
    }

    async void OnDestroy()
    {
        shouldReconnect = false;
        await DisconnectFromServer();
    }

    async void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            shouldReconnect = false;
            await DisconnectFromServer();
        }
        else if (autoConnect)
        {
            shouldReconnect = true;
            await ConnectToServer();
        }
    }

    public async System.Threading.Tasks.Task ConnectToServer()
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            Log("Already connected");
            return;
        }

        try
        {
            Log($"Connecting to: {serverUrl}");

            webSocket = new WebSocket(serverUrl);

            webSocket.OnOpen += OnWebSocketOpen;
            webSocket.OnError += OnWebSocketError;
            webSocket.OnClose += OnWebSocketClose;
            webSocket.OnMessage += OnWebSocketMessage;

            await webSocket.Connect();
        }
        catch (Exception e)
        {
            LogError($"Connection failed: {e.Message}");

            if (shouldReconnect)
            {
                StartCoroutine(ReconnectAfterDelay());
            }
        }
    }

    public async System.Threading.Tasks.Task DisconnectFromServer()
    {
        if (webSocket != null)
        {
            try
            {
                await webSocket.Close();
                webSocket = null;
                Log("Disconnected from WebSocket server");
            }
            catch (Exception e)
            {
                LogError($"Error disconnecting: {e.Message}");
            }
        }
    }

    private void OnWebSocketOpen()
    {
        Log("✅ Connected to WebSocket server!");
    }

    private void OnWebSocketError(string error)
    {
        LogError($"❌ WebSocket error: {error}");

        if (shouldReconnect && gameObject.activeInHierarchy)
        {
            StartCoroutine(ReconnectAfterDelay());
        }
    }

    private void OnWebSocketClose(WebSocketCloseCode closeCode)
    {
        Log($"🔌 WebSocket closed: {closeCode}");

        if (shouldReconnect && gameObject.activeInHierarchy)
        {
            StartCoroutine(ReconnectAfterDelay());
        }
    }

    private void OnWebSocketMessage(byte[] data)
    {
        string message = System.Text.Encoding.UTF8.GetString(data);
        Log($"📨 Received: {message}");
        ProcessCommand(message);
    }

    private IEnumerator ReconnectAfterDelay()
    {
        Log($"Attempting to reconnect in {reconnectDelay} seconds...");
        yield return new WaitForSeconds(reconnectDelay);

        if (shouldReconnect && gameObject.activeInHierarchy)
        {
            _ = ConnectToServer(); // Fire and forget async call
        }
    }

    public void ProcessCommand(string command)
    {
        command = command.Trim().ToLower();

        switch (command)
        {
            case "left":
                player.ChangeLane(-1);
                Log("🡸 ChangeLane left executed");
                break;
            case "right":
                player.ChangeLane(1);
                Log("🡺 ChangeLane right executed");
                break;
            case "jump":
            case "up":
                player.Jump();
                Log("⬆️ Jump executed");
                break;
            case "slide":
            case "down":
                player.Slide();
                Log("⬇️ Slide executed");
                break;
            case "start":
                SceneManager.LoadScene("Main");
                Log("Start Game");
                break;
            default:
                LogError($"❓ Unknown command: {command}");
                break;
        }
    }

    private void Log(string message)
    {
        if (enableLogging)
        {
            Debug.Log($"[WebSocket] {message}");
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[WebSocket] {message}");
    }

    public bool IsConnected => webSocket?.State == WebSocketState.Open;
    public string Status => webSocket?.State.ToString() ?? "Disconnected";
}
