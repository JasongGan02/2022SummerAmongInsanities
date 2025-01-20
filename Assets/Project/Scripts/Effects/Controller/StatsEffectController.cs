using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StatsEffectController : EffectController
{
    protected StatsEffectObject statsEffectObject;

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
}