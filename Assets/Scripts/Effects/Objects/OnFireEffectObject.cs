using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Stats/OnFire")]
public class OnFireEffectObject : StatsEffectObject
{
    [Header("On Fire Effect Object")] 
    public int maxStacks = 999; // Upper limit for the number of stacks
}