using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EffectDebugManager : MonoBehaviour
{
    // A list of EffectObject assets that you want to manage.
    private IList<EffectObject> effectObjects;

    // Using async void Start is acceptable in Unity for initialization.
    private async void Start()
    {
        // Load all assets labeled "EffectObject" using Addressables.
        effectObjects = await AddressablesManager.Instance.LoadMultipleAssetsAsync<EffectObject>("EffectObject");

        if (effectObjects == null)
        {
            Debug.LogError("Failed to load EffectObject assets.");
        }
        else
        {
            Debug.Log($"Loaded {effectObjects.Count} EffectObject assets.");
        }
    }

    // This method resets each effect to level 1 by calling its ResetUpgrade() method.
    public void ResetAllEffects()
    {
        if (effectObjects == null)
        {
            Debug.LogWarning("Effect objects have not been loaded yet.");
            return;
        }

        foreach (var effect in effectObjects)
        {
            if (effect != null)
            {
                effect.ResetUpgrade();
                Debug.Log($"Reset {effect.name} to level 1.");
            }
        }
    }
}