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

    protected override async void OnInitializeEffect()
    {
        // 1) Load all WallObject assets with the key/label "WallObject" using Addressables.
        var walls = await AddressablesManager.Instance.LoadMultipleAssetsAsync<WallObject>("WallObject");
        if (walls == null || walls.Count == 0)
        {
            Debug.LogWarning("No wall objects found with label/key 'WallObject'.");
            return;
        }

        // 2) Iterate through each WallObject.
        foreach (var wall in walls)
        {
            // Check if an on-fire effect of the same type as onFireEffectObject is already present.
            var existingEffect = wall.onHitEffects.Find(effect =>
                effect.GetType() == onFireEffectObject.GetType());

            if (existingEffect == null)
            {
                // If no matching effect is found, add this effect (or the onFireEffectObject instance).
                wall.onHitEffects.Add(this);
            }
        }
    }
}