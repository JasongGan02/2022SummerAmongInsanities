using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectEvents : MonoBehaviour
{
    public static event Action<EffectObject, Type> OnEffectApplied;
    public static event Action<EffectObject, IEffectableController> OnEffectRemoved;

    public static void ApplyEffect(EffectObject effect, Type target)
    {
        OnEffectApplied?.Invoke(effect, target);
    }

    public static void RemoveEffect(EffectObject effect, IEffectableController target)
    {
        OnEffectRemoved?.Invoke(effect, target);
    }
}
