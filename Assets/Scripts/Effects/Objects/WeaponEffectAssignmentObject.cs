using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Effects/WeaponEffectAssignment")]
public class WeaponEffectAssignmentObject : EffectObject
{
    [Header("Weapon Specific Settings")]
    public EffectObject targetOnHitEffectObject;
}
