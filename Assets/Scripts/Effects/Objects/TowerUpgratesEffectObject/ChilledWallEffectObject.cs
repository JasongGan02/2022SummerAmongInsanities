using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Tower Upgrades/ChilledWallEffectObject")]
public class ChilledWallEffectObject : EffectObject
{
    [Header("ChilledWallEffectObject Fields")] 
    public int maxStacks;
    public float armorMultiplier;
    
    
    public override async void InitializeEffectObject()
    {
        // Load all wall objects
        var walls = await AddressablesManager.Instance.LoadMultipleAssetsAsync<WallObject>("WallObject");
        if (walls == null || walls.Count == 0)
        {
            Debug.LogWarning("No wall objects found with label/key 'WallObject'.");
            return;
        }

        foreach (var wall in walls)
        {
            var existingEffect = wall.onHitEffects.Find(effect => effect.GetType() == GetType());
            if (existingEffect == null)
            {
                wall.onHitEffects.Add(this);
            }
            else
            {
                //((IUpgradeableEffectObject)existingEffect).UpgradeLevel();
            }
        }
    }
}
