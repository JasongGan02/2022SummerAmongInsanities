using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StatsEffectController : EffectController
{
    protected float dHP;
    
    

    protected override IEnumerator DestroyAfterDuration()
    {
        float elapsedTime = 0f;

        while (elapsedTime < effectObject.duration)
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
        this.gameObject.GetComponent<CharacterController>().ApplyHPChange(-amount);
    }
}
