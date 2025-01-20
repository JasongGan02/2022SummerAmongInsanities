using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Effects/EffectLevelConfig")]
public class EffectConfiguration : ScriptableObject
{
    public string effectName; // Effect name, e.g., "Burning Effect", "Freezing Effect"
    public List<LevelStats> levels;
    
    public LevelStats GetStatsForLevel(int level)
    {
        return levels.Find(l => l.level == level) ?? levels[^1]; // Return the highest level as fallback
    }
    
    public float GetAttribute(string attributeName, int level, float defaultValue = 0f)
    {
        var stats = GetStatsForLevel(level);
        if (stats != null)
        {
            foreach (var attribute in stats.attributes)
            {
                if (attribute.Key == attributeName)
                {
                    return attribute.Value;
                }
            }
        }
        Debug.LogWarning($"Attribute '{attributeName}' not found for level {level} in {effectName}.");
        return defaultValue;
    }
    
}

[Serializable]
public class LevelStats
{
    public int level;
    public List<KeyValuePair> attributes = new List<KeyValuePair>(); 
}

[Serializable]
public class KeyValuePair
{
    public string Key; // Attribute name
    public float Value; // Attribute value
}