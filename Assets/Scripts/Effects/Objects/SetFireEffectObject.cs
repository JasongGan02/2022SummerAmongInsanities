using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/WeaponEffect/SetFire")]
public class SetFireEffectObject : EffectObject
{
    [Header("SetFireEffectObject Fields")]
    public OnFireEffectObject onFireEffectObject;
    [Range(0, 1)] public float chance;
    public int stacksPerHit;


    public override void ExecuteEffect(IEffectableController effectedGameController) //apply effect on a single object
    {
        if (Random.value <= chance) // Check chance to apply
        {
            onFireEffectObject.ApplyMultipleStacks(effectedGameController, stacksPerHit);
        }
    }

    protected override async void OnInitializeEffect()
    {
        // Apply the effect to all weapon objects
        var allWeaponObjects = await AddressablesManager.Instance.LoadMultipleAssetsAsync<WeaponObject>("WeaponObject");
        foreach (var weaponObject in allWeaponObjects)
        {
            if (!weaponObject.onHitEffects.Contains(this))
            {
                weaponObject.onHitEffects.Add(this);
                //Debug.Log($"Added {name} to weapon object: {weaponObject.name}");
            }
            //TODO: if weapon objcet is ranged, then the projectile will carry the effect.
        }
    }
}