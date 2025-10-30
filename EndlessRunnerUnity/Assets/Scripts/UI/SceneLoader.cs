using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene()
    {
		Debug.Log("Button clicked! Loading 'Main' scene...");
        SceneManager.LoadScene("Main");
    }
}
