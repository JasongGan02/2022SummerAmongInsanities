using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Effects/Stats")]
public class StatsEffectObject : EffectObject
{
    [Header("Stats Effect Object")] 
    public CharacterStats statChanges;
    public bool isMultiply = false;
    
}
