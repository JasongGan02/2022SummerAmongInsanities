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
        
        characterController.ChangeMaxStats(statsEffectObject.statChanges);
    }
}