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
    }

    protected override IEnumerator EffectDurationCoroutine()
    {
        float elapsedTime = 0f;
        float tickDuration = effectObject.tickDuration > 0 ? effectObject.tickDuration : Time.deltaTime;

        //Debug.Log("Starting effect with duration: " + effectObject.duration);
        while (elapsedTime < effectObject.duration && TimeSystemManager.Instance.IsRedMoon)
        {
            if (this == null || !gameObject.activeInHierarchy)
            {
                EndEffect(); // Call a method to handle cleanup
                yield break; // Exit the coroutine
            }

            if (Time.timeScale == 0f)
            {
                //Debug.Log("Time.timeScale is 0, waiting for time to resume.");
                yield return new WaitUntil(() => Time.timeScale > 0f); // Wait for timeScale to resume
            }

            //Debug.Log("Effect progress: " + elapsedTime + "/" + effectObject.duration);
            DuringEffect(); // Trigger the effect logic
            yield return new WaitForSeconds(tickDuration); // Wait for the tick duration
            elapsedTime += tickDuration; // Increment elapsed time by tick duration
        }

        EndEffect();
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