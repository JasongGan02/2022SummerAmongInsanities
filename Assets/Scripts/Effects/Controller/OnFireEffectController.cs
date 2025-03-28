using System;
using System.Collections;
using UnityEngine;

public class OnFireEffectController : DurationCurrentStatChangeEffectController, IDamageSource
{
    public int stackCount = 1; // Tracks the current number of stacks
    private OnFireEffectObject onFireEffectObject;

    public override void Initialize(EffectObject effectObject)
    {
        onFireEffectObject = effectObject as OnFireEffectObject;
        if (onFireEffectObject == null)
        {
            Type type = effectObject.GetType();
            if (type.IsSubclassOf(typeof(OnFireEffectObject)))
            {
                onFireEffectObject = (OnFireEffectObject)Convert.ChangeType(effectObject, type);
            }
        }
        base.Initialize(effectObject);
    }

    protected override void HandleStacking()
    {
        var existingEffects = GetComponents<OnFireEffectController>();
        foreach (var effect in existingEffects)
        {
            if (effect != this) // Skip the newly added instance (this)
            {
                Debug.Log($"Stacked on {effect.GetInstanceID()}");
                effect.AddStack(1); // Add stack to the existing effect
                enabled = false; // Disable this instance
                Destroy(this); // Destroy the new instance
                return; // Exit early to prevent further execution
            }
        }

        // No existing stackable effect was found
        //Debug.Log($"First effect applied: {GetInstanceID()}");
    }

    protected override void DuringEffect()
    {
        if (!enabled) // Skip execution if the component is disabled
        {
            Debug.LogWarning($"DuringEffect skipped for instance {GetInstanceID()} (component disabled).");
            return;
        }

        IDamageable iDamageable = GetComponent<IDamageable>();
        if (iDamageable == null)
        {
            Debug.LogError("Not a damageable");
            return;
        }

        // Scale damage by stack count
        ApplyDamage(iDamageable);

    }
    
    public GameObject SourceGameObject { get; }
    public void ApplyDamage(IDamageable target)
    {
        float damagePerTick = statsEffectObject.statChanges.hp * stackCount;
        target.TakeDamage(target.CalculateDamage(damagePerTick, 0f, 0f), this);
    }

    public void AddStack(int stacksToAdd)
    {
        if (stackCount >= onFireEffectObject.maxStacks)
        {
            ResetEffectDuration();
            return;
        }

        // Increment the stack count
        int stacksAdded = Mathf.Clamp(stackCount + stacksToAdd, 0, onFireEffectObject.maxStacks) - stackCount;
        stackCount += stacksAdded;
        UpdateVFX(stackCount);

        // Refresh the duration
        ResetEffectDuration();
    }
}
