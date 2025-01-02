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
                bool isUpgraded = false;
                foreach (WeaponObject weapon in allWeapons)
                {
                    var existingEffect = weapon.onHitEffects.Find(effect =>
                        effect.GetType() == weaponEffectAssignment.targetOnHitEffectObject.GetType());
                    
                    
                    if (existingEffect == null)
                    {
                        AddNewEffect(weapon, weaponEffectAssignment);
                    }
                    else if (effectObject.isStackable && !isUpgraded)
                    {
                        HandleStacking(existingEffect);
                        isUpgraded = true;
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
        Debug.Log($"Adding new effect to weapon: {weapon.itemName}");
    }


    private void HandleStacking(EffectObject existingEffect)
    {
        if (existingEffect is IUpgradeableEffectObject existingStatsEffect)
        {
            existingStatsEffect.UpgradeLevel();
            Debug.Log($"Upgraded stats of existing effect on weapon to level {existingStatsEffect.CurrentLevel}");
        }
    }
}
