using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    protected EffectObject effectObject;
    protected List<GameObject> activeVFXList = new List<GameObject>(); // Track active VFX instances
    protected const int maxVFXStack = 3; // Maximum VFX stack limit

    public virtual void Initialize(EffectObject effectObject)
    {
        this.effectObject = effectObject;
        OnEffectStarted();
    }

    protected virtual void OnEffectStarted()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"Cannot start effect on inactive GameObject: {gameObject.name}");
            return;
        }
        
        if (effectObject.isStackable){
            HandleStacking();
        }
        else
        {
            // Handle non-stackable behavior
            HandleNonStackable();
        }
        
        //Preparation like special effect or one time instant change
        StartEffect();
        StartVFX();
        
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"Cannot start effect on inactive GameObject: {gameObject.name}");
            return;
        }
        //Duration
        StartCoroutine(EffectDurationCoroutine());
    }
    
    protected virtual void StartEffect()
    {
        // Perform any necessary updates here
    }
    
    protected virtual void StartVFX()
    {
        if (effectObject.vfxPrefab != null)
        {
            if (activeVFXList.Count >= maxVFXStack)
            {
                // If we've reached the max VFX stack, remove or reuse the oldest
                GameObject oldestVFX = activeVFXList[0];
                activeVFXList.RemoveAt(0);
                Debug.Log($"Removing oldest VFX: {oldestVFX.name}");
                Destroy(oldestVFX); // Destroy the oldest VFX
            }

            // Spawn a new VFX instance
            Transform parentTransform = effectObject.attachToTarget ? transform : null;
            GameObject newVFX = Instantiate(effectObject.vfxPrefab, transform.position, Quaternion.identity, parentTransform);
            activeVFXList.Add(newVFX); // Add to the active VFX list

            Debug.Log($"Created new VFX: {newVFX.name} | Total Active VFX: {activeVFXList.Count}");
        }
    }

    
    
    protected virtual IEnumerator EffectDurationCoroutine()
    {
        float elapsedTime = 0f;
        float tickDuration = effectObject.tickDuration > 0 ? effectObject.tickDuration : Time.deltaTime;

        //Debug.Log("Starting effect with duration: " + effectObject.duration);
        while (elapsedTime < effectObject.duration)
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
    
    protected virtual void DuringEffect()
    {
        // Perform any necessary updates here if the effect is not a one-time effect
    }
    
    protected virtual void ResetEffect()
    {
        // Reset any temporary effect-related stats or variables here
    }
    
    protected virtual void EndEffect()
    {
        if (effectObject.requiresReset)
            ResetEffect(); // Reset temporary changes if needed
        
        // Destroy all active VFX
        foreach (var vfx in activeVFXList)
        {
            if (vfx != null)
            {
                Debug.Log($"Destroying VFX: {vfx.name}");
                vfx.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Encountered a null VFX in activeVFXList.");
            }
        }
        activeVFXList.Clear();

        Debug.Log($"All VFX destroyed for effect: {effectObject.name}");
        
        if (!effectObject.isPermanent)
            Destroy(this); // Destroy the EffectController if it's not permanent
    }
    
    protected virtual void HandleStacking()
    {
        // Override this function in derived classes to specify stacking behavio
        Debug.LogWarning($"Stacking is enabled, but no stacking logic implemented for effect: {effectObject.name}");
    }
    
    protected virtual void HandleNonStackable()
    {
        var existingControllers = GetComponents<EffectController>();

        foreach (var controller in existingControllers)
        {
            // If another instance of the same type exists and it's not this one
            if (controller != this && controller.GetType() == GetType())
            {
                Debug.Log($"Non-stackable effect already exists. Resetting duration for: {controller}");
                controller.ResetEffectDuration();
                Destroy(this); // Destroy the current instance
                return;
            }
        }
    }
    
    protected virtual void ResetEffectDuration()
    {
        Debug.Log($"Resetting duration for effect: {this}");
        StopAllCoroutines();
        StartCoroutine(EffectDurationCoroutine());
    }


    public virtual void OnObjectInactive()
    {
        StopAllCoroutines();
        if (effectObject.isPermanent)
        {
            return;
        }
        Destroy(this);
    }

}