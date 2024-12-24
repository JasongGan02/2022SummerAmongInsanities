using UnityEngine;

public class WeaponEffectAssignmentController : EffectController
{
    protected override void OnEffectStarted()
    {
        if (effectObject is WeaponEffectAssignmentObject weaponEffectAssignment)
        {
            WeaponObject[] allWeapons = FindAllWeapons();
            if (allWeapons != null && allWeapons.Length > 0)
            {
                foreach (WeaponObject weapon in allWeapons)
                {
                    var existingEffect = weapon.onHitEffects.Find(effect =>
                        effect.GetType() == weaponEffectAssignment.targetOnHitEffectObject.GetType());

                    if (existingEffect != null)
                    {
                        HandleStacking(existingEffect, weaponEffectAssignment);
                    }
                    else
                    {
                        AddNewEffect(weapon, weaponEffectAssignment);
                    }
                }
            }
            else
            {
                Debug.LogWarning("No weapons found in the specified path!");
            }
        }
        else
        {
            Debug.LogError("EffectObject is not of type WeaponEffectAssignmentObject!");
        }
    }

    private WeaponObject[] FindAllWeapons()
    {
        return Resources.LoadAll<WeaponObject>("Weapons/WeaponObjects");
    }

    private void AddNewEffect(WeaponObject weapon, WeaponEffectAssignmentObject assignmentObject)
    {
        weapon.onHitEffects.Add(assignmentObject.targetOnHitEffectObject);

        // Clone stats to avoid shared state issues
        var clonedStats = new CharacterStats();
        clonedStats.CopyFrom(assignmentObject.GetCurrentLevelStats().levelStats);

        ((StatsEffectObject)assignmentObject.targetOnHitEffectObject).statChanges = clonedStats;
        assignmentObject.targetOnHitEffectObject.duration = assignmentObject.GetCurrentLevelStats().duration;
        Debug.Log($"Adding new effect to weapon: {weapon.itemName}");
    }


    private void HandleStacking(EffectObject existingEffect, WeaponEffectAssignmentObject assignmentObject)
    {
        if (existingEffect is StatsEffectObject existingStatsEffect)
        {
            assignmentObject.UpgradeLevel();
            var clonedStats = new CharacterStats();
            clonedStats.CopyFrom(assignmentObject.GetCurrentLevelStats().levelStats);
            existingStatsEffect.statChanges = clonedStats;
            assignmentObject.targetOnHitEffectObject.duration = assignmentObject.GetCurrentLevelStats().duration;
            Debug.Log($"Upgraded stats of existing effect on weapon to level {assignmentObject.currentLevel}");
        }
    }
}
