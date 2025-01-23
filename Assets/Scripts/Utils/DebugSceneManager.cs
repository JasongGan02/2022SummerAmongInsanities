using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugSceneManager : MonoBehaviour
{
    // List of scenes in the build settings
    [SerializeField] private string[] sceneNames;

    private static DebugSceneManager instance;

    private void Awake()
    {
        // Ensure only one instance exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    private void Update()
    {
        // Check if the Ctrl key is held down
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            for (int i = 1; i <= sceneNames.Length; i++)
            {
                // Check for numeric key press (1, 2, 3, ...)
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    LoadScene(i - 1); // Subtract 1 to match array index
                }
            }
        }
    }

    private void LoadScene(int index)
    {
        if (index >= 0 && index < sceneNames.Length)
        {
            string sceneName = sceneNames[index];
            if (!string.IsNullOrEmpty(sceneName))
            {
                Debug.Log($"Loading Scene: {sceneName}");
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning($"Scene name at index {index} is empty.");
            }
        }
        else
        {
            Debug.LogWarning($"Invalid scene index: {index}");
        }
    }
}