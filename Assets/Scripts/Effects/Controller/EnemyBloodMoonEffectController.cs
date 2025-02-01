using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBloodMoonEffectController : StatsEffectController
{
    protected override void StartEffect()
    {
        // Perform any necessary updates here
        CharacterController characterController = GetComponent<CharacterController>();

        if (characterController == null)
        {
            Debug.LogError("Max Stats not attached to a CC");
            return;
        }

        if (statsEffectObject.isMultiply)
            characterController.MultiplyCurrentStats(statsEffectObject.statChanges);
        else
            characterController.AddCurrentStats(statsEffectObject.statChanges);

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
            characterController.MultiplyCurrentStats(statsEffectObject.statChanges.Inverse());
        else
            characterController.AddCurrentStats(-statsEffectObject.statChanges);
    }
}
