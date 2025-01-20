using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Wall Upgrades/BurningWallEffectObject")]
public class BurningWallEffectObject : EffectObject
{
    [Header("BurningWallEffectObject Fields")]
    public OnFireEffectObject onFireEffectObject;
    public int stacksPerHit = 1;
    public float effectInterval = 2f; // Apply burn every x seconds

    public override void ExecuteEffect(IEffectableController effectedGameController)
    {
        onFireEffectObject.ApplyMultipleStacks(effectedGameController, stacksPerHit);
    }
    
    public override void InitializeEffectObject()
    {
        // Load all wall objects
        WallObject[] walls = LoadAllAssets<WallObject>();
        if (walls == null || walls.Length == 0)
        {
            Debug.LogWarning("No wall objects found.");
            return;
        }

        foreach (var wall in walls)
        {
            var existingEffect = wall.onHitEffects.Find(effect => effect.GetType() == onFireEffectObject.GetType());
            if (existingEffect == null)
            {
                wall.onHitEffects.Add(this);
            }
            else
            {
                ((IUpgradeableEffectObject)existingEffect).UpgradeLevel();
            }
        }
    }

    private T[] LoadAllAssets<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        List<T> assets = new List<T>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) assets.Add(asset);
        }

        return assets.ToArray();
    }
        
}
