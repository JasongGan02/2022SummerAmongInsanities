using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StatsEffectController : EffectController
{
    private StatsEffectObject statsEffectObject;

    public override void Initialize(EffectObject effectObject)
    {
        statsEffectObject = effectObject as StatsEffectObject;

        if (statsEffectObject == null)
        {
            Type type = effectObject.GetType();
            if (type.IsSubclassOf(typeof(StatsEffectObject)))
            {
                statsEffectObject = (StatsEffectObject)Convert.ChangeType(effectObject, type);
            }
        }
        
        base.Initialize(effectObject);
    }

    protected override void StartEffect()
    {
        if (statsEffectObject != null)
        {
            this.gameObject.GetComponent<CharacterController>().ChangeCurrentStats(statsEffectObject.statChanges);
        }
    }

    protected override void DuringEffect()
    {
        // Perform any necessary updates here if the effect is not a one-time effect
    }
}