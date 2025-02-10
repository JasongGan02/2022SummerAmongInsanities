using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Stats/OnFire")]
public class OnFireEffectObject : StatsEffectObject
{
    [Header("On Fire Effect Object")] 
    [Tooltip("Upper limit for the number of stacks.")] 
    public int maxStacks = 999; // Upper limit for the number of stacks
    
    public void ApplyMultipleStacks(IEffectableController target, int stacksToAdd)
    {
        if (target == null) return;

        var onFire = (target as MonoBehaviour).GetComponent<OnFireEffectController>();

        if (onFire == null)
        {
            ExecuteEffect(target);
            stacksToAdd--;
            onFire = (target as MonoBehaviour).GetComponent<OnFireEffectController>();
        }
        
        onFire.AddStack(stacksToAdd);
    }
}