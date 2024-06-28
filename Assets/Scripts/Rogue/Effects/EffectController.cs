using System.Collections;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    protected EffectObject effectObject;

    public virtual void Initialize(EffectObject effectObject)
    {
        this.effectObject = effectObject;
        StartEffect();
    }

    protected virtual void StartEffect()
    {
        if (effectObject.duration > 0 && !effectObject.isPermanent)
        {
            StartCoroutine(DestroyAfterDuration());
        }
    }

    protected virtual IEnumerator DestroyAfterDuration()
    {
        float elapsedTime = 0f;
        while (elapsedTime < effectObject.duration)
        {
            DuringEffect();
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ResetEffect();
        Destroy(this);
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