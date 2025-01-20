using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Stats/OnFrozen")]
public class OnFrozenEffectObject : EffectObject
{
    [Header("On Frozen Effect Object")] 
    public float cooldownDuration = 1f; // 1-second cooldown
}