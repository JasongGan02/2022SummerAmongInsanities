using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Tower Upgrades/ChilledArrowTowerEffectObject")]
public class ChilledArrowTowerEffectObject : EffectObject
{
    [Header("ChilledArrowTowerEffectObject Fields")]
    public OnChilledEffectObject onChilledEffectObject;
    public int stacksPerHit;

    public override void ExecuteEffect(IEffectableController effectedGameController)
    {
        onChilledEffectObject.ApplyMultipleStacks(effectedGameController, stacksPerHit);
    }

    protected override async void OnInitializeEffect()
    {
        // Load the target tower object asynchronously.
        var archerTower = await AddressablesManager.Instance.LoadAssetAsync<RangedTowerObject>("Assets/ScriptableObjects/CharacterObjects/TowerObject/ArcherTower.asset");

        if (archerTower != null)
        {
            // Check if an effect of this type already exists on the tower's projectile.
            var existingEffect = archerTower.projectileObject.onHitEffects.Find(effect =>
                effect.GetType() == GetType());

            if (existingEffect == null)
            {
                // No matching effect found; add this effect.
                archerTower.projectileObject.onHitEffects.Add(this);
            }
            else
            {
                // A matching effect already exists; upgrade it.
                ((ChilledArrowTowerEffectObject)existingEffect).UpgradeEffect();
            }
        }
    }
}