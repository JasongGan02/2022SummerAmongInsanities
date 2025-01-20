using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class WallController : TowerController
{
    public override void TakeDamage(float amount, IDamageSource damageSource)
    {
        base.TakeDamage(amount, damageSource);
        foreach (var effect in ((WallObject)characterObject).onHitEffects)
        {
            if (effect is ChilledWallEffectObject)
            {
                effect.ExecuteEffect(GetComponent<CharacterController>());
            }
            else
            {
                effect.ExecuteEffect(damageSource as IEffectableController);
            }
            
        }
    }
}
