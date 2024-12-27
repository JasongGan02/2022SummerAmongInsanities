using System;
using System.Collections;
using UnityEngine;

public class OnFrozenEffectController : EffectController
{
    private Rigidbody2D rb2D;
    private bool wasKinematic;
    private Vector2 storedVelocity2D;
    private OnFrozenEffectObject onFrozenEffectObject;
    
    public override void Initialize(EffectObject effectObject)
    {
        onFrozenEffectObject = effectObject as OnFrozenEffectObject;
        if (onFrozenEffectObject == null)
        {
            Type type = effectObject.GetType();
            if (type.IsSubclassOf(typeof(OnFrozenEffectObject)))
            {
                onFrozenEffectObject = (OnFrozenEffectObject)Convert.ChangeType(effectObject, type);
            }
        }
        base.Initialize(effectObject);
    }
    
    protected override void StartEffect()
    {
        // Freeze enemy behavior
        var enemyController = GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.IsFrozen = true;
        }

        // Attempt to freeze 2D Rigidbody
        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D != null)
        {
            storedVelocity2D = rb2D.velocity; // Save current velocity
            rb2D.velocity = Vector2.zero;    // Stop movement
            wasKinematic = rb2D.isKinematic;
            rb2D.isKinematic = true;         // Freeze Rigidbody
        }

        Debug.Log($"Object frozen for {effectObject.duration} seconds.");
    }

    protected override void ResetEffect()
    {
        // Unfreeze enemy behavior
        var enemyController = GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.IsFrozen = false;
        }

        // Restore 2D Rigidbody
        if (rb2D != null)
        {
            rb2D.isKinematic = wasKinematic; // Restore kinematic state
            rb2D.velocity = storedVelocity2D; // Restore velocity
        }

        Debug.Log("Object unfrozen.");
    }
    
    protected override IEnumerator EffectDurationCoroutine()
    {
        float elapsedTime = 0f;
        float tickDuration = effectObject.tickDuration > 0 ? effectObject.tickDuration : Time.deltaTime;

        Debug.Log("Starting effect with duration: " + effectObject.duration);
        while (elapsedTime < effectObject.duration)
        {
            if (this == null || !gameObject.activeInHierarchy)
            {
                EndEffect(); // Call a method to handle cleanup
                yield break; // Exit the coroutine
            }
            
            if (Time.timeScale == 0f)
            {
                Debug.Log("Time.timeScale is 0, waiting for time to resume.");
                yield return new WaitUntil(() => Time.timeScale > 0f); // Wait for timeScale to resume
            }
            Debug.Log("Effect progress: " + elapsedTime + "/" + effectObject.duration);
            DuringEffect(); // Trigger the effect logic
            yield return new WaitForSeconds(tickDuration); // Wait for the tick duration
            elapsedTime += tickDuration; // Increment elapsed time by tick duration

        }

        EndEffect();
    }

    private void StartCountDown()
    {
        StartCoroutine(FrozenCDRoutine());
    }
    
    private IEnumerator FrozenCDRoutine()
    {
        yield return new WaitForSeconds(onFrozenEffectObject.cooldownDuration);
    }
    
    protected virtual void EndEffect()
    {
        if (effectObject.requiresReset)
            ResetEffect(); // Reset temporary changes if needed
        Debug.Log("unfrozen");
        StartCountDown(); // during this CD wont stack frozen
        Debug.Log("cd down");
        if (!effectObject.isPermanent)
            Destroy(this); // Destroy the EffectController if it's not permanent
    }

    protected override void HandleNonStackable()
    {
        // No stacking allowed for frozen effect
        Debug.Log("Frozen effect is non-stackable. Skipping stacking logic.");
        Destroy(this);
    }
}
