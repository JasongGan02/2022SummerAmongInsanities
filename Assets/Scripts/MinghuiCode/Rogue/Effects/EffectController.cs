using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Threading;
using UnityEditor.Build;

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
        // Start the effect or perform any necessary setup

        // Start a coroutine to wait for the specified duration and then destroy the component
        StartCoroutine(DestroyAfterDuration());
    }

    protected virtual System.Collections.IEnumerator DestroyAfterDuration()
    {
        float elapsedTime = 0f;

        while (elapsedTime < effectObject.duration)
        {
            // Perform any necessary updates here if the effect is not a one-time effect
 
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //yield return new WaitForSeconds(duration);

        // Reset any temporary effect-related stats or variables here

        // Destroy the component from the effected controller after the duration has elapsed
        if (!effectObject.isPermanent)
            Destroy(this);
    }
}
