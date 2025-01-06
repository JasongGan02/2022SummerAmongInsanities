using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : TowerController
{
    public override void TakeDamage(float amount, IDamageSource damageSource)
    {
        base.TakeDamage(amount, damageSource);
        foreach (var effect in ((WallObject)characterObject).onHitEffects)
        {
            effect.ExecuteEffect(damageSource as IEffectableController);
        }
    }
}
