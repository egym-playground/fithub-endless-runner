# Game Over Menu System

This system provides a complete Game Over menu that appears when the player loses all lives, allowing them to restart the game or return to the main menu.

## Features

- **Automatic Game Over Detection**: Triggers when player lives reach 0
- **Smooth Animations**: Optional scaling animation for the game over panel
- **Score Display**: Shows final score and coins collected
- **Restart Functionality**: Allows players to restart the current level
- **Main Menu Navigation**: Return to the main menu scene
- **Time Pause**: Automatically pauses the game when game over is shown

## Setup Instructions

### Option 1: Manual Setup (Recommended)

1. **Add GameOverMenu Component**:
   - In your Main scene, find or create a Canvas
   - Add the `GameOverMenu` component to any GameObject in the scene

2. **Create Game Over UI**:
   - Create a Panel as a child of your Canvas
   - Add the following UI elements as children of the panel:
     - Text component for "Game Over" title
     - Text component for final score display
     - Text component for final coins display  
     - Button for "Restart" functionality
     - Button for "Main Menu" functionality

3. **Assign References**:
   - In the GameOverMenu component, assign all the UI elements to their respective fields
   - Make sure the gameOverPanel is assigned to the main panel

### Option 2: Automatic Setup (Quick Start)

1. **Add Auto-Creator**:
   - Find your Canvas in the Main scene
   - Add the `GameOverUICreator` component to the Canvas
   - The UI will be automatically created when you play the scene

### Existing Integration

The system is already integrated with:
- **Player.cs**: Automatically triggers game over when lives reach 0
- **UIManager.cs**: Enhanced with game over functionality
- **SceneLoader.cs**: Can be used for scene transitions

## How It Works

1. **Game Over Trigger**: When `Player.HitObstacles()` reduces lives to 0:
   - Player movement stops (`runSpeed = 0`)
   - Death animation plays (`animator.SetBool("Dead", true)`)
   - `GameOver()` coroutine starts

2. **Game Over Display**: After 2 seconds (death animation time):
   - Game over panel becomes visible
   - Final score and coins are displayed
   - Game time is paused (`Time.timeScale = 0f`)
   - Optional animation plays

3. **Player Actions**:
   - **Restart**: Reloads the current scene
   - **Main Menu**: Loads the "Initial" scene
   - Both actions resume normal time scale

## Customization Options

### GameOverMenu Settings
```csharp
[Header("Animation Settings")]
public bool useAnimation = true;        // Enable/disable show animation
public float animationDuration = 0.5f;  // Animation duration in seconds
```

### GameOverUICreator Settings
```csharp
[Header("UI Styling")]
public Color backgroundColor = new Color(0, 0, 0, 0.8f);  // Panel background
public Color textColor = Color.white;                      // Text color
public Color buttonColor = new Color(0.2f, 0.6f, 1f);     // Button color
```

## Scene Requirements

Make sure your scenes are properly named:
- **Main Scene**: The gameplay scene (where Player.cs is active)
- **Initial Scene**: The main menu scene (named "Initial")

## Troubleshooting

### Game Over UI Not Showing
1. Check that GameOverMenu component is in the scene
2. Ensure gameOverPanel is assigned in the inspector
3. Verify UI elements are properly assigned

### Restart Not Working
1. Confirm the current scene is named correctly
2. Check that SceneManager.LoadScene permissions are set
3. Ensure the scene is included in Build Settings

### Main Menu Not Loading
1. Verify there is a scene named "Initial"
2. Make sure "Initial" scene is added to Build Settings
3. Check for any SceneManager errors in the console

## Files Modified

- `Assets/Scripts/Player.cs` - Added game over detection and coroutine
- `Assets/Scripts/UIManager.cs` - Enhanced with game over menu functionality
- `Assets/Scripts/UI/SceneLoader.cs` - Scene loading functionality
- `Assets/Scripts/UI/GameOverMenu.cs` - Main game over menu component (NEW)
- `Assets/Scripts/UI/GameOverUICreator.cs` - Automatic UI creation helper (NEW)

## Dependencies

- UnityEngine.UI
- UnityEngine.SceneManagement
- LeanTween (optional, for animations - can be removed if not available)
