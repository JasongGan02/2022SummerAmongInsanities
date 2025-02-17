using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/WeaponEffect/SetChilled")]
public class SetChilledEffectObject : EffectObject
{
    [Header("SetChilledEffectObject Fields")]
    public OnChilledEffectObject onChilledEffectObject;
    public int stacksPerHit;

    public override void ExecuteEffect(IEffectableController effectedGameController) //apply effect on a single object
    {
        onChilledEffectObject.ApplyMultipleStacks(effectedGameController, stacksPerHit);
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