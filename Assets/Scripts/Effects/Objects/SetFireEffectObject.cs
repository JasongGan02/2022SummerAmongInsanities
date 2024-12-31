using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/WeaponEffect/SetFire")]
public class SetFireEffectObject : EffectObject, IUpgradeableEffectObject
{
    [Header("SetFireEffectObject Fields")] 
    public OnFireEffectObject onFireEffectObject;
    [Range(0, 1)] public float chance;
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

       
        chance = LevelConfig.GetAttribute("chance", targetLevel, 0f);
        stacksPerHit = (int) LevelConfig.GetAttribute("stacksPerHit", targetLevel, 0f);
    }

    #endregion

    public override void ExecuteEffect(IEffectableController effectedGameController) //apply effect on a single object
    {
        if (Random.value <= chance) // Check chance to apply
        {
            ApplyBurnEffect(effectedGameController);
        }
    }
    
    private void ApplyBurnEffect(IEffectableController effectableController)
    {
        for (int i = 0; i < stacksPerHit; i++)
        {
            onFireEffectObject.ExecuteEffect(effectableController); 
        }
    }
    
    private void OnValidate()
    {
        if (CurrentLevel < 1)
        {
            CurrentLevel = 1; // Ensure minimum level is 1
        }

        UpdateStatsToTargetLevel(CurrentLevel);
    }
}
