using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Effects/WeaponEffectAssignment")]
public class WeaponEffectAssignmentObject : EffectObject
{
    [Header("Weapon Specific Settings")]
    public EffectObject targetOnHitEffectObject;
    
    public override async void InitializeEffectObject()
    {
        // Apply the effect to all weapon objects
        var allWeaponObjects = await AddressablesManager.Instance.LoadMultipleAssetsAsync<WeaponObject>("WeaponObject");
        foreach (var weaponObject in allWeaponObjects)
        {
            if (!weaponObject.onInitializeEffects.Contains(this))
            {
                weaponObject.onInitializeEffects.Add(this);
                //Debug.Log($"Added {name} to weapon object: {weaponObject.name}");
            }
            //TODO: if weapon objcet is ranged, then the projectile will carry the effect.
        }
        // Apply the effect to all existing weapon instances in the scene
        Weapon[] activeWeapons = Object.FindObjectsOfType<Weapon>();
        foreach (var weapon in activeWeapons)
        {
            AddEffectToWeaponInstance(weapon);
        }
    }

    private void AddEffectToWeaponInstance(Weapon weapon)
    {
        // Check if the weapon already has the effect
        var effectController = weapon.GetComponent<DetonateChargeEffectController>();
        if (effectController == null)
        {
            // Add the DetonateChargeEffectController to the weapon instance
            var controller = weapon.gameObject.AddComponent<DetonateChargeEffectController>();
            controller.Initialize(this);
            //Debug.Log($"Added {name} effect to weapon instance: {weapon.name}");
        }
    }
}
