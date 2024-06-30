using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StatsEffectController : EffectController
{
    protected float dHP;
    
    protected override void DuringEffect()
    {
        // Perform any necessary updates here if the effect is not a one-time effect
    }
    private void CharacterCurrentHPChange(float amount)
    {
        Type type = effectObject.GetApplyingControllerType();
        this.gameObject.GetComponent<CharacterController>().ApplyHPChange(-amount);
    }
}
