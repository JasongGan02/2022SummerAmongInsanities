using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnChilledEffectController : StatsEffectController
{
    public int stackCount = 1; // Tracks the current number of stacks
    
    private OnChilledEffectObject onChilledEffectObject;

    public override void Initialize(EffectObject effectObject)
    {
        onChilledEffectObject = effectObject as OnChilledEffectObject;
        if (onChilledEffectObject == null)
        {
            Type type = effectObject.GetType();
            if (type.IsSubclassOf(typeof(OnChilledEffectObject)))
            {
                onChilledEffectObject = (OnChilledEffectObject)Convert.ChangeType(effectObject, type);
            }
        }
        base.Initialize(effectObject);
    }
    
    protected override void HandleStacking()
    {
        var existingEffects = GetComponents<OnChilledEffectController>();
        foreach (var effect in existingEffects)
        {
            if (effect != this) // Skip the newly added instance (this)
            {
                //Debug.Log($"Stacked on {effect.GetInstanceID()}");
                effect.AddStack(); // Add stack to the existing effect
                enabled = false; // Disable this instance
                Destroy(this); // Destroy the new instance
                return; // Exit early to prevent further execution
            }
        }

        // No existing stackable effect was found
        //Debug.Log($"First effect applied: {GetInstanceID()}");
    }
    
    protected override void StartEffect()
    {
        if (!enabled) // Skip execution if the component is disabled
        {
            //Debug.LogWarning($"StartEffect skipped for instance {GetInstanceID()} (component disabled).");
            return;
        }

        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
           //Debug.LogError("Max Stats not attached to a CC");
            return;
        }
        //Debug.Log($"StartEffect on {GetInstanceID()}, current stack: {stackCount}");
        characterController.MultiplyCurrentStats(statsEffectObject.statChanges);
    }

    
    protected override void ResetEffect()
    {
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("CharacterController is not attached to the GameObject!");
            return;
        }

        //Debug.Log("Resetting effect...");

        // Calculate the cumulative factor for all stacks
        float inverseFactor = Mathf.Pow(statsEffectObject.statChanges.movingSpeed, stackCount);

        Debug.Log($"Reverting movementSpeed by factor: {1 / inverseFactor}");

        // Apply the inverse factor to reset movement speed
        var inverseStats = new CharacterStats
        {
            movingSpeed = 1 / inverseFactor // Revert movement speed
        };

        characterController.MultiplyCurrentStats(inverseStats);

        //Debug.Log($"After reset: {characterController.CurrentStats}");
    }

    
    private void AddStack()
    {
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("CharacterController is not attached!");
            return;
        }

        stackCount++;
        
        // If at max stacks, trigger the frozen effect
        if (stackCount >= onChilledEffectObject.maxStacks)
        {
            TryTriggerFrozenEffect();
            return;
        }
        

        // Increment the stack count and apply the effect
        //Debug.Log($"AddStack called on {GetInstanceID()}. Stack added. Current stacks: {stackCount}");
        characterController.MultiplyCurrentStats(statsEffectObject.statChanges);

        ResetEffectDuration();
    }
    
    private void TryTriggerFrozenEffect()
    {
        Debug.Log("Triggering frozen effect.");
        onChilledEffectObject.onFrozenEffectObject.ExecuteEffect(GetComponent<CharacterController>());
        EndEffect();
    }

}