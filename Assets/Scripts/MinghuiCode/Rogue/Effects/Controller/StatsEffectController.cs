using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StatsEffectController : EffectController
{
    protected float dHP;
    
    
    public override void Initialize(EffectObject effectObject)
    {
        base.Initialize(effectObject);
        StartEffect();
    }

    protected override void StartEffect()
    {
        // Start the effect or perform any necessary setup

        // Start a coroutine to wait for the specified duration and then destroy the component
        StartCoroutine(DestroyAfterDuration());
    }

    protected override System.Collections.IEnumerator DestroyAfterDuration()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Perform any necessary updates here if the effect is not a one-time effect
            CharacterCurrentHPChange(dHP*Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //yield return new WaitForSeconds(duration);

        // Reset any temporary effect-related stats or variables here

        // Destroy the component after the duration has elapsed
        Destroy(this);
    }

    private void CharacterCurrentHPChange(float amount)
    {
        Type type = effectObject.GetApplyingControllerType();
        (GetComponent(type) as CharacterController).takenDamage(-amount);
    }
}
