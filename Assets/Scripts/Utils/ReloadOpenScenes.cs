using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This is a utility script that reloads all open scenes. Add this component to a GameObject
/// in one of your scenes, and be sure there aren't other components on this GameObject.
/// </summary>
public class ReloadOpenScenes : MonoBehaviour
{
    [Tooltip("Pressing the combination of these keys will reload all currently open scenes.")]
    [SerializeField]
    private List<KeyCode> KeyCodes = new()
    {
        KeyCode.LeftCommand,
        KeyCode.LeftShift,
        KeyCode.R
    };

    // This is the Singleton design pattern. Ensure there's only ever one of these components.
    private static ReloadOpenScenes instance = null;

    // Only one reload operation should happen at a time because the unload/load operatins
    // are asynchronous.
    private static bool bIsReloadingScenes = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            // The reload process may create a new GameObject with this component. If that
            // happens, just destroy this component.
            DestroyImmediate(this);
        }
    }

    private void Update()
    {
        if (bIsReloadingScenes || KeyCodes.Count == 0)
        {
            return;
        }

        bool bAreKeyCodesPressed = true;
        foreach (KeyCode key in KeyCodes)
        {
            bAreKeyCodesPressed &= Input.GetKey(key);
        }

        if (bAreKeyCodesPressed)
        {
            bIsReloadingScenes = true;
            StartCoroutine(ReloadAllOpenScenes());
        }
    }

    private IEnumerator ReloadAllOpenScenes()
    {
        Debug.Log("Reloading scenes...");

        // Only one scene can be the active scene. Remember that scene, and record
        // the other scenes that are open.
        string activeScenePath = SceneManager.GetActiveScene().path;
        List<string> nonActiveScenePaths = new();
        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; ++sceneIndex)
        {
            string scenePath = SceneManager.GetSceneAt(sceneIndex).path;
            if (scenePath != activeScenePath)
            {
                nonActiveScenePaths.Add(scenePath);
            }
        }

        // Unload all scenes except the active scene.
        Debug.Log("Unloadng scenes...");
        foreach (string scenePath in nonActiveScenePaths)
        {
            Debug.Log($"\tUnloading scene {scenePath}...");
            yield return SceneManager.UnloadSceneAsync(scenePath);
        }

        // Reload all scenes. Load the active scene with the single mode, and load
        // all other scenes additively.
        Debug.Log("Loading scenes...");
        Debug.Log($"\tReloading active scene {activeScenePath}...");
        yield return SceneManager.LoadSceneAsync(activeScenePath, LoadSceneMode.Single);
        foreach (string scenePath in nonActiveScenePaths)
        {
            Debug.Log($"\tLoading scene {scenePath}...");
            yield return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
        }

        Debug.Log("Finished reloading scenes!");

        // Update state to make sure we can reload again.
        bIsReloadingScenes = false;
    }
}