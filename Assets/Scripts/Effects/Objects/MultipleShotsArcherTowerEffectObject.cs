using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(menuName = "Effects/Tower Upgrades/MultipleShotsArcherTowerEffectObject")]
public class MultipleShotsArcherTowerEffectObject : EffectObject
{
    [Header("MultipleShotsArcherTowerEffectObject Fields")]
    public int projectilesPerShot = 1;
    
    public override void InitializeEffectObject()
    {
        // Load the target tower object
        RangedTowerObject archerTower = LoadAssetByName<RangedTowerObject>("ArcherTower");
        archerTower.rangedTowerStats.projectilesPerShot += projectilesPerShot;

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
