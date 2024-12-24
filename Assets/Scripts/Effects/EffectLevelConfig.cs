using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Effects/EffectLevelConfig")]
public class EffectLevelConfig : ScriptableObject
{

    [Header("Effect Level Configurations")]
    public List<LevelStats> levels = new List<LevelStats>();

    public LevelStats GetStatsForLevel(int level)
    {
        // Find the stats for the given level or return the highest level as fallback
        return levels.Find(stats => stats.level == level) ?? levels[levels.Count - 1];
    }
}

[Serializable]
public class LevelStats
{
    public int level;
    [FormerlySerializedAs("deltaStats")] public CharacterStats levelStats; // Damage dealt per second
    public float duration; // Effect duration
}
