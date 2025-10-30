using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene()
    {
		Debug.Log("Button clicked! Loading 'Main' scene...");
        SceneManager.LoadScene("Main");
    }

    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name cannot be null or empty!");
            return;
        }

        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
