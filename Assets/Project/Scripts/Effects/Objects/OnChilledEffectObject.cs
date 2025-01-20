using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Stats/OnChilled")]
public class OnChilledEffectObject : StatsEffectObject
{
    [Header("On Chilled Effect Object")] 
    public int maxStacks = 20; // Upper limit for the number of stacks
    public EffectObject onFrozenEffectObject;
    
    public void ApplyMultipleStacks(IEffectableController target, int stacksToAdd)
    {
        if (target == null) return;

        var onChilled = (target as MonoBehaviour).GetComponent<OnChilledEffectController>();

        if (onChilled == null)
        {
            ExecuteEffect(target);
            stacksToAdd--;
            onChilled = (target as MonoBehaviour).GetComponent<OnChilledEffectController>();
            Debug.Log("on fire current stack: " + onChilled.stackCount);
            Debug.Log("stacksToAdd: " + stacksToAdd);
        }
        
        onChilled.AddStack(stacksToAdd);
        Debug.Log("on fire current stack: " + onChilled.stackCount);
    }
}
