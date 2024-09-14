using UnityEngine;
using UnityEngine.UIElements;

public class TemporaryMaxStatChangeEffectController : StatsEffectController
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

    protected override void ResetEffect()
    {
        CharacterController characterController = GetComponent<CharacterController>();
        
        if (characterController == null)
        {
            Debug.LogError("Max Stats not attached to a CC");
            return;
        }
        
        characterController.ChangeMaxStats(-statsEffectObject.statChanges);
    }
}