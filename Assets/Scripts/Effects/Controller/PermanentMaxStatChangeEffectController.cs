using UnityEngine;

public class PermanentMaxStatChangeEffectController : StatsEffectController
{
    protected override void StartEffect()
    {
        CharacterController characterController = GetComponent<CharacterController>();
        
        if (characterController == null)
        {
            Debug.LogError("Max Stats not attached to a CC");
            return;
        }

        if (statsEffectObject.isMultiply)
            characterController.MultiplyMaxStats(statsEffectObject.statChanges);
        else
            characterController.AddMaxStats(statsEffectObject.statChanges);
    }
}
