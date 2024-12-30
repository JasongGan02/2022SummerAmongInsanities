using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Stats/OnFire")]
public class OnFireEffectObject : StatsEffectObject, IUpgradeableEffectObject
{
    [Header("On Fire Effect Object")] 
    [Tooltip("Upper limit for the number of stacks.")] 
    public int maxStacks = 999; // Upper limit for the number of stacks

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

       
        statChanges.hp = LevelConfig.GetAttribute("damagePerSecond", targetLevel, 0f);
        duration = LevelConfig.GetAttribute("duration", targetLevel, 0f);
    }

    #endregion
}