using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Wall Upgrades/BurningWallEffectObject")]
public class BurningWallEffectObject : EffectObject
{
    [Header("BurningWallEffectObject Fields")]
    public OnFireEffectObject onFireEffectObject;
    public int stacksPerHit = 1;
    public float effectInterval = 2f; // Apply burn every x seconds

    public override void ExecuteEffect(IEffectableController effectedGameController)
    {
        onFireEffectObject.ApplyMultipleStacks(effectedGameController, stacksPerHit);
    }
    
    public override async void InitializeEffectObject()
    {
        // 1) Load all wall objects (returns an IList<WallObject>)
        var walls = await AddressablesManager.Instance.LoadMultipleAssetsAsync<WallObject>("WallObject");
        if (walls == null || walls.Count == 0)
        {
            Debug.LogWarning("No wall objects found with label/key 'WallObject'.");
            return;
        }

        // 2) Iterate through each WallObject
        foreach (var wall in walls)
        {
            // If onHitEffects is a list of IEffectObject or something similar,
            // find an existing effect matching the type of 'onFireEffectObject'.
            var existingEffect = wall.onHitEffects.Find(effect =>
                effect.GetType() == onFireEffectObject.GetType());

            if (existingEffect == null)
            {
                // Here, decide whether to add `this` or `onFireEffectObject`
                // If 'this' is your effect object, do:
                //    wall.onHitEffects.Add(this);
                // If you want to add the onFireEffectObject instance, do:
                //    wall.onHitEffects.Add(onFireEffectObject);

                wall.onHitEffects.Add(this); 
            }
            else
            {
                // If the existing effect implements IUpgradeableEffectObject,
                // call UpgradeLevel on it.
                ((IUpgradeableEffectObject)existingEffect).UpgradeLevel();
            }
        }
    }

}
