using System.Collections;
using UnityEngine;

public abstract class EffectController : MonoBehaviour
{
    protected EffectObject effectObject;

    public virtual void Initialize(EffectObject effectObject)
    {
        this.effectObject = effectObject;
        OnEffectStarted();
    }

    protected virtual void OnEffectStarted()
    {
        //Preparation like special effect or one time instant change
        StartEffect();
        
        //Duration
        StartCoroutine(EffectDurationCoroutine());
        
        //Reset if temporary
        if (effectObject.requiresReset)
            ResetEffect();
        
        //Destroy if not permanent
        if (!effectObject.isPermanent)
            Destroy(this);
    }
    
    protected virtual void StartEffect()
    {
        // Perform any necessary updates here
    }
    
    protected virtual IEnumerator EffectDurationCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < effectObject.duration)
        {
            DuringEffect();
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    
    protected virtual void DuringEffect()
    {
        // Perform any necessary updates here if the effect is not a one-time effect
    }
    
    protected virtual void ResetEffect()
    {
        // Reset any temporary effect-related stats or variables here
    }
}