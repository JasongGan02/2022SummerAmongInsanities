using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Tower Upgrades/BurningArrowTowerEffectObject")]
public class BurningArrowTowerEffectObject : EffectObject
{
    [Header("BurningArrowTowerEffectObject Fields")]
    public OnFireEffectObject onFireEffectObject;
    public int stacksPerHit;

    // -----------------------------------------------------------
    // Make the InitializeEffectObject async so we can 'await' loads
    // -----------------------------------------------------------
    protected override async void OnInitializeEffect()
    {
        // Load the target tower object from Addressables
        RangedTowerObject archerTower = await AddressablesManager.Instance.LoadAssetAsync<RangedTowerObject>("Assets/ScriptableObjects/CharacterObjects/TowerObject/ArcherTower.asset");

        if (archerTower != null)
        {
            var existingEffect = archerTower.projectileObject.onHitEffects.Find(effect =>
                effect.GetType() == onFireEffectObject.GetType());

            if (existingEffect == null)
            {
                archerTower.projectileObject.onHitEffects.Add(onFireEffectObject);
            }

            // Set the specific stacks for OnFireEffectObject
            archerTower.projectileObject.SetEffectStacks(onFireEffectObject, stacksPerHit);
        }
        else
        {
            Debug.LogWarning("Could not find ArcherTower via Addressables.");
        }
    }
}