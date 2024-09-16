using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DurationCurrentStatChangeEffectController : StatsEffectController
{
    protected override void DuringEffect()
    {
        // Perform any necessary updates here if the effect is not a one-time effect
        CharacterController characterController = GetComponent<CharacterController>();
        
        if (characterController == null)
        {
            Debug.LogError("Max Stats not attached to a CC");
            return;
        }
        
        characterController.ChangeCurrentStats(statsEffectObject.statChanges);
    }
}
