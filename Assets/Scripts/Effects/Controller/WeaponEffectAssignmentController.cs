using UnityEngine;

public class WeaponEffectAssignmentController : EffectController
{
    private bool firstTimeAdded = true;

    protected override void OnEffectStarted()
    {
        // Cast the effectObject to WeaponEffectAssignmentObject
        if (effectObject is WeaponEffectAssignmentObject weaponEffectAssignment)
        {
            // Find all weapon objects
            WeaponObject[] allWeapons = FindAllWeapons();
            if (allWeapons != null && allWeapons.Length > 0)
            {
                foreach (WeaponObject weapon in allWeapons)
                {
                    // Check if the weapon already has the effect
                    var existingEffect = weapon.onHitEffects.Find(effect =>
                        effect.GetType() == weaponEffectAssignment.targetOnHitEffectObject.GetType());

                    if (firstTimeAdded)
                    {
                        AddNewEffect(weapon, weaponEffectAssignment.targetOnHitEffectObject);
                        firstTimeAdded = false;
                       
                    }
                    else
                    {
                        // Upgrade the effect by increasing damage over time
                        UpgradeEffect(existingEffect);
                       
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
        // Use Resources to load all WeaponObjects
        return Resources.LoadAll<WeaponObject>("Weapons/WeaponObjects"); // Adjust the path as needed
    }

    private void AddNewEffect(WeaponObject weapon, EffectObject newEffect)
    {
        // Configure the new effect with base damage
        if (newEffect is StatsEffectObject statsEffect)
        {
            // Assign the effect
            weapon.onHitEffects.Add(newEffect);
            Debug.Log($"Adding new 'on fire' effect to weapon: {weapon.itemName}");
        }

    }

    private void UpgradeEffect(EffectObject existingEffect)
    {
        // Upgrade the effect only once
        if ((effectObject as WeaponEffectAssignmentObject)?.targetOnHitEffectObject is StatsEffectObject statsEffect)
        {
            statsEffect.statChanges.hp += ((WeaponEffectAssignmentObject)effectObject).repeatingStats; // Reduce target HP by base damage per second
            Debug.Log($"Upgrading 'on fire' effect globally: New damage per second = {statsEffect.statChanges.hp}");
        }
        else
        {
            Debug.LogWarning("The provided effect is not of type StatsEffectObject.");
        }
    }
}
