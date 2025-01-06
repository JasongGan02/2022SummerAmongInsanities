using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Wall Upgrades/BurningWallEffectObject")]
public class BurningWallEffectObject : EffectObject, IUpgradeableEffectObject
{
    [Header("BurningWallEffectObject Fields")]
    public OnFireEffectObject onFireEffectObject;
    public int stacksPerHit = 1;
    public float effectInterval = 2f; // Apply burn every x seconds

    public override void ExecuteEffect(IEffectableController effectedGameController)
    {
        onFireEffectObject.ApplyMultipleStacks(effectedGameController, stacksPerHit);
    }
    
    public override void ExecuteEffectOnAType()
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

    #region IUpgradeableEffect Fields

    [Header("Upgradeable Effect Configuration")]
    [SerializeField] private EffectConfiguration levelConfig;
    [SerializeField] private int currentLevel = 1;

    public EffectConfiguration LevelConfig
    {
        get => levelConfig;
        set => levelConfig = value;
    }

    public int CurrentLevel
    {
        get => currentLevel;
        set => currentLevel = value;
    }

    public void UpdateStatsToTargetLevel(int targetLevel)
    {
        if (LevelConfig == null)
        {
            Debug.LogError("LevelConfig is not set.");
            return;
        }

        var stats = LevelConfig.GetStatsForLevel(targetLevel);
        if (stats == null)
        {
            Debug.LogWarning($"No stats found for level {targetLevel} in {name}.");
            return;
        }

        stacksPerHit = (int)LevelConfig.GetAttribute("stacksPerHit", targetLevel, 1);
            //effectInterval = LevelConfig.GetAttribute("effectInterval", targetLevel, 2f);
    }

    private void OnValidate()
    {
        if (CurrentLevel < 1) CurrentLevel = 1;
        UpdateStatsToTargetLevel(CurrentLevel);
    }

    #endregion
}
