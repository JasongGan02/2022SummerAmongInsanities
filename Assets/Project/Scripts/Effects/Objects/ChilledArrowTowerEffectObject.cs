using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Tower Upgrades/ChilledArrowTowerEffectObject")]
public class ChilledArrowTowerEffectObject : EffectObject
{
    [Header("ChilledArrowTowerEffectObject Fields")]
    public OnChilledEffectObject onChilledEffectObject;
    public int stacksPerHit;
    
    public override void ExecuteEffect(IEffectableController effectedGameController)
    {
        onChilledEffectObject.ApplyMultipleStacks(effectedGameController, stacksPerHit);
    }
    
    public override void InitializeEffectObject()
    {
        // Load the target tower object
        RangedTowerObject archerTower = LoadAssetByName<RangedTowerObject>("ArcherTower");

        if (archerTower != null)
        {
            var existingEffect = archerTower.projectileObject.onHitEffects.Find(effect =>
                effect.GetType() == GetType());

            if (existingEffect == null)
            {
                archerTower.projectileObject.onHitEffects.Add(this);
            }
            else
            {
                (this as IUpgradeableEffectObject).UpgradeLevel();
            }
        }
    }
    
    private T LoadAssetByName<T>(string assetName) where T : ScriptableObject
    {
        // Use AssetDatabase.FindAssets to find assets of the specified type
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            // Match the asset name (excluding path and file extension)
            if (asset != null && asset.name.Equals(assetName, StringComparison.OrdinalIgnoreCase))
            {
                return asset; // Return the matching asset
            }
        }

        Debug.LogWarning($"No asset of type {typeof(T).Name} found with name '{assetName}'");
        return null; // Return null if no matching asset is found
    }
}
