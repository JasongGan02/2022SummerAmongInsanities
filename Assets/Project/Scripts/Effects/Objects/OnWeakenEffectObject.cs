using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/OnHitEffect/OnWeakenEffectObject")]
public class OnWeakenEffectObject : EffectObject
{
    [Header("OnWeakenEffectObject Fields")]
    public float damageReductionMultiplier;
}
