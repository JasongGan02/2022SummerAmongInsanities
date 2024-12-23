using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Stats/OnChilled")]
public class OnChilledEffectObject : StatsEffectObject
{
    [Header("On Chilled Effect Object")] 
    public int maxStacks = 20; // Upper limit for the number of stacks
    public EffectObject onFrozenEffectObject;
}
