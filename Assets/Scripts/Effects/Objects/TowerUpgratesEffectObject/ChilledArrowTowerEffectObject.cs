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
    
    public override async void InitializeEffectObject()
    {
        // Load the target tower object
        var archerTower = await AddressablesManager.Instance.LoadAssetAsync<RangedTowerObject>("Assets/ScriptableObjects/CharacterObjects/TowerObject/ArcherTower.asset");

        if (archerTower != null)
        {
            var existingEffect = archerTower.projectileObject.onHitEffects.Find(effect =>
                effect.GetType() == GetType());

            if (existingEffect == null)
            {
                archerTower.projectileObject.onHitEffects.Add(this);
            }
            else
            {
                (this as IUpgradeableEffectObject).UpgradeLevel();
            }
        }
    }
    
}
