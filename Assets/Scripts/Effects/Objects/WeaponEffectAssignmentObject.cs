using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/WeaponEffectAssignment")]
public class WeaponEffectAssignmentObject : EffectObject
{
    [Header("Weapon Specific Settings")]
    public EffectObject targetOnHitEffectObject;
    public EffectLevelConfig levelConfig; // Reference to level-based stats
    public int currentLevel = 1; // Track current level
    
    public LevelStats GetCurrentLevelStats()
    {
        return levelConfig.GetStatsForLevel(currentLevel);
    }
    
    public void UpgradeLevel()
    {
        if (currentLevel < levelConfig.levels.Count)
        {
            currentLevel++;
        }
    }
}
