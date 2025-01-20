using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class OnDetonateEffectController : EffectController
{
    
    private OnDetonateEffectObject onDetonateEffectObject;

    public override void Initialize(EffectObject effectObject)
    {
        onDetonateEffectObject = effectObject as OnDetonateEffectObject;
        if (onDetonateEffectObject == null)
        {
            Type type = effectObject.GetType();
            if (type.IsSubclassOf(typeof(OnDetonateEffectObject)))
            {
                onDetonateEffectObject = (OnDetonateEffectObject)Convert.ChangeType(effectObject, type);
            }
        }
        base.Initialize(effectObject);
    }
    
    protected override void StartEffect()
    {
        var onFireEffect = GetComponent<OnFireEffectController>();
        if (onFireEffect != null)
        {
            int stacks = onFireEffect.stackCount;
            var target = GetComponent<CharacterController>();
            float criticalMultiplier = target.CurrentStats.criticalMultiplier;
            var damage = stacks * onDetonateEffectObject.onFireEffectObject.statChanges.hp;
            var finalDamage = ((IDamageable)target).CalculateDamage(damage, onDetonateEffectObject.chance,criticalMultiplier);
            target.TakeDamage(finalDamage, null);
        }
    }
}
