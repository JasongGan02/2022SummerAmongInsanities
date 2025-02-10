using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Effects/Weapon/DetonateChargeEffect")]
public class DetonateChargeEffectObject : EffectObject
{
    [Header("Detonate Charge Properties")]
    public float chargeDuration; // Time required to fully charge
    public OnDetonateEffectObject onDetonateEffectObject; // Reference to the detonate effect

    public override void ExecuteEffect(IEffectableController effectedGameController)
    {
        var weapon = effectedGameController as MonoBehaviour;
        var effectController = weapon.GetComponent<DetonateChargeEffectController>();
        if (effectController == null)
        {
            // Add the DetonateChargeEffectController to the weapon instance
            var controller = weapon.gameObject.AddComponent<DetonateChargeEffectController>();
            controller.Initialize(this);
        }
    }


    protected override async void OnInitializeEffect()
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
        Weapon[] activeWeapons = FindObjectsOfType<Weapon>();
        foreach (var weapon in activeWeapons)
        {
            AddEffectToWeaponInstance(weapon);
        }
    }

    protected override void OnUpgradeEffect()
    {
        onDetonateEffectObject.UpgradeEffect();
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