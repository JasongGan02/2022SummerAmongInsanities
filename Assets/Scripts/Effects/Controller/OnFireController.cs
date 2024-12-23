using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFireController : DurationCurrentStatChangeEffectController
{
    protected override void DuringEffect()
    {
        // Perform any necessary updates here if the effect is not a one-time effect
        IDamageable iDamageable = GetComponent<IDamageable>();
        
        if (iDamageable == null)
        {
            Debug.LogError("Not a damageable");
            return;
        }

        iDamageable.TakeDamage(statsEffectObject.statChanges.hp, GetComponent<CharacterController>() as IDamageSource);
    }
}
