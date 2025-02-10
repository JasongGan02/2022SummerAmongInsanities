using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Effects/OnHitEffect/OnDetonate")]
public class OnDetonateEffectObject : EffectObject
{
    [Header("DetonateEffectObject Fields")]
    public float detonateDamageMultiplier;
    public float criticalChance;
    public OnFireEffectObject onFireEffectObject;
}