using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Tower Upgrades/BurningArrowTowerEffectObject")]
public class BurningArrowTowerEffectObject : EffectObject, IUpgradeableEffectObject
{
    [Header("BurningArrowTowerEffectObject Fields")]
    public OnFireEffectObject onFireEffectObject;
    public int stacksPerHit;
    
    
    #region IUpgradeableEffect Fields

    [Header("Upgradeable Effect Configuration")]
    [Tooltip("Configuration object containing level-based stats.")]
    [SerializeField] private EffectConfiguration levelConfig; // Reference to level stats

    [Tooltip("The current level of the effect.")]
    [SerializeField] private int currentLevel = 1; // Tracks the current level of the effect

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
        
        stacksPerHit = (int) LevelConfig.GetAttribute("stacksPerHit", targetLevel, 0f);
    }
    
    private void OnValidate()
    {
        if (CurrentLevel < 1)
        {
            CurrentLevel = 1; // Ensure minimum level is 1
        }

        UpdateStatsToTargetLevel(CurrentLevel);
    }
    
    #endregion
    
    // -----------------------------------------------------------
    // Make the InitializeEffectObject async so we can 'await' loads
    // -----------------------------------------------------------
    public override async void InitializeEffectObject()
    {
        // Load the target tower object from Addressables
        RangedTowerObject archerTower = await AddressablesManager.Instance.LoadAssetAsync<RangedTowerObject>("Assets/ScriptableObjects/CharacterObjects/TowerObject/ArcherTower.asset");

        if (archerTower != null)
        {
            var existingEffect = archerTower.projectileObject.onHitEffects.Find(effect =>
                effect.GetType() == onFireEffectObject.GetType());

            if (existingEffect == null)
            {
                archerTower.projectileObject.onHitEffects.Add(onFireEffectObject);
            }
            else
            {
                (this as IUpgradeableEffectObject).UpgradeLevel();
            }

            // Set the specific stacks for OnFireEffectObject
            archerTower.projectileObject.SetEffectStacks(onFireEffectObject, stacksPerHit);
        }
        else
        {
            Debug.LogWarning("Could not find ArcherTower via Addressables.");
        }
    }
}
