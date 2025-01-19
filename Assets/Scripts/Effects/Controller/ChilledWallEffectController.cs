using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChilledWallEffectController : EffectController
{
    public int stackCount = 0;
    
    private ChilledWallEffectObject chilledWallEffectObject;    
    
    public override void Initialize(EffectObject effectObject)
    {
        chilledWallEffectObject = effectObject as ChilledWallEffectObject;
        if (chilledWallEffectObject == null)
        {
            Type type = effectObject.GetType();
            if (type.IsSubclassOf(typeof(ChilledWallEffectObject)))
            {
                chilledWallEffectObject = (ChilledWallEffectObject)Convert.ChangeType(effectObject, type);
            }
        }
        base.Initialize(effectObject);
    }
    
    protected override void StartEffect()
    {
        if (!enabled) // Skip execution if the component is disabled
        {
            //Debug.LogWarning($"StartEffect skipped for instance {GetInstanceID()} (component disabled).");
            return;
        }
        
        //Debug.Log($"StartEffect on {GetInstanceID()}, current stack: {stackCount}");
        AddStack(1);
    }

    protected override void HandleStacking()
    {
        var existingEffects = GetComponents<ChilledWallEffectController>();
        foreach (var effect in existingEffects)
        {
            if (effect != this) // Skip the newly added instance (this)
            {
                //Debug.Log($"Stacked on {effect.GetInstanceID()}");
                effect.AddStack(1); // Add stack to the existing effect
                enabled = false; // Disable this instance
                Destroy(this); // Destroy the new instance
                return; // Exit early to prevent further execution
            }
        }

        // No existing stackable effect was found
        //Debug.Log($"First effect applied: {GetInstanceID()}");
    }
    
    protected override void ResetEffect()
    {
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("CharacterController is not attached to the GameObject!");
            return;
        }
        float armorToRemove = chilledWallEffectObject.armorMultiplier * stackCount;
        
        CharacterStats characterStats = new CharacterStats { armor = -armorToRemove };
        characterController.AddCurrentStats(characterStats);
        //Debug.Log($"After reset: {characterController.CurrentStats}");
    }

    private void AddStack(int stacksToAdd)
    {
        if (stackCount >= chilledWallEffectObject.maxStacks)
        {
            ResetEffectDuration();
            return;
        }

        // Increment the stack count
        stackCount = Mathf.Clamp(stackCount + stacksToAdd, 0, chilledWallEffectObject.maxStacks);
        float armorToAdd = chilledWallEffectObject.armorMultiplier * stacksToAdd;
        CharacterStats characterStats = new CharacterStats { armor = armorToAdd };
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            //Debug.LogError("Max Stats not attached to a CC");
            return;
        }
        // Debug.Log("Before: " + characterController.CurrentStats);
        characterController.AddCurrentStats(characterStats);
        // Debug.Log("After: " + characterController.CurrentStats);
        // Refresh the duration
        ResetEffectDuration();
    }
}
