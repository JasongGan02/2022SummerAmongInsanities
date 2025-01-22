using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AddressablesManager : MonoBehaviour
{
    private static AddressablesManager _instance;
    public static AddressablesManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var managerGO = new GameObject(nameof(AddressablesManager));
                _instance = managerGO.AddComponent<AddressablesManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ---------------------------------------------------------------
    // 1) Default approach: returns T or any subclass assignable to T
    // ---------------------------------------------------------------
    public async Task<IList<T>> LoadMultipleAssetsAsync<T>(object keyOrLabel)
    {
        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(keyOrLabel, null);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        else
        {
            Debug.LogError($"[AddressablesManager] Failed to load assets with key/label '{keyOrLabel}'.");
            return null;
        }
    }

    // -----------------------------------------------------------------
    // 2) Exact-type approach: excludes derived classes of T
    // -----------------------------------------------------------------
    public async Task<IList<T>> LoadMultipleAssetsExactTypeAsync<T>(object keyOrLabel)
        where T : UnityEngine.Object
    {
        // Load as 'Object' to get all raw assets under that key/label.
        AsyncOperationHandle<IList<Object>> handle = Addressables.LoadAssetsAsync<Object>(keyOrLabel, null);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            List<T> matchedAssets = new List<T>();

            foreach (var asset in handle.Result)
            {
                // Ensure it's not null and type is exactly T
                if (asset != null && asset.GetType() == typeof(T))
                {
                    // Now you can safely cast because T must derive from UnityEngine.Object
                    matchedAssets.Add((T)asset);
                }
            }

            return matchedAssets;
        }
        else
        {
            Debug.LogError($"[AddressablesManager] Failed to load assets with key/label '{keyOrLabel}'.");
            return null;
        }
    }
    
    public async Task<T> LoadAssetAsync<T>(object keyOrLabel)
    {
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(keyOrLabel);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        else
        {
            Debug.LogError($"[AddressablesManager] Failed to load asset '{keyOrLabel}'.");
            return default;
        }
    }

    public async Task<GameObject> InstantiateAsync(object keyOrLabel, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(keyOrLabel, position, rotation, parent);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        else
        {
            Debug.LogError($"[AddressablesManager] Failed to instantiate '{keyOrLabel}'.");
            return null;
        }
    }

    public void Release<T>(T loadedAsset)
    {
        Addressables.Release(loadedAsset);
    }
}
