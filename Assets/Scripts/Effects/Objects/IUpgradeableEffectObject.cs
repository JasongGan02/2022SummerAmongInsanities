using System;
using UnityEngine;

/// <summary>
/// Interface for effects that support level-based upgrades.
/// </summary>
public interface IUpgradeableEffectObject
{
    public EffectConfiguration LevelConfig { get; set; }
    
    public int CurrentLevel { get; set; }
    
    public void UpgradeLevel()
    {
        if (LevelConfig != null && CurrentLevel < LevelConfig.levels.Count)
        {
            CurrentLevel++;
            UpdateStatsToTargetLevel(CurrentLevel);
        }
        else
        {
            Debug.LogWarning($"{this} cannot be upgraded further.");
        }
    }

    /// <summary>
    /// Updates the effect stats to match the specified target level.
    /// </summary>
    /// <param name="targetLevel">The target level to update the stats to.</param>
    void UpdateStatsToTargetLevel(int targetLevel);
}