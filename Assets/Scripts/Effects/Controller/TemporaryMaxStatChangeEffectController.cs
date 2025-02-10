using UnityEngine;
using UnityEngine.UIElements;

public class TemporaryMaxStatChangeEffectController : StatsEffectController
{
    protected override void StartEffect()
    {
        CharacterController characterController = GetComponent<CharacterController>();

        if (characterController == null)
        {
            Debug.LogError("Max Stats not attached to a Character controller");
            return;
        }


        if (statsEffectObject.isMultiply)
            characterController.MultiplyMaxStats(statsEffectObject.statChanges);
        else
            characterController.AddMaxStats(statsEffectObject.statChanges);

        if (characterController is EnemyController enemyController)
        {
            Debug.Log("RedMoon After Stats: " + enemyController + " " + enemyController.CurrentStats);
        }
    }

    protected override void ResetEffect()
    {
        CharacterController characterController = GetComponent<CharacterController>();

        if (characterController == null)
        {
            Debug.LogError("Max Stats not attached to a CC");
            return;
        }

        if (statsEffectObject.isMultiply)
            characterController.MultiplyMaxStats(statsEffectObject.statChanges.Inverse());
        else
            characterController.AddMaxStats(-statsEffectObject.statChanges);
    }
}