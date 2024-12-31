using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Effects/OnHitEffect/OnDetonate")]
public class OnDetonateEffectObject : EffectObject, IUpgradeableEffectObject
{
    [Header("DetonateEffectObject Fields")] 
    public float detonateDamageMultiplier;
    public float chance;
    public OnFireEffectObject onFireEffectObject;
    
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
        
        chance = LevelConfig.GetAttribute("chance", targetLevel, 0f);
        detonateDamageMultiplier = (int) LevelConfig.GetAttribute("detonateDamageMultiplier", targetLevel, 0f);
    }

    #endregion
    
    private void OnValidate()
    {
        if (CurrentLevel < 1)
        {
            CurrentLevel = 1; // Ensure minimum level is 1
        }

        UpdateStatsToTargetLevel(CurrentLevel);
    }
}
