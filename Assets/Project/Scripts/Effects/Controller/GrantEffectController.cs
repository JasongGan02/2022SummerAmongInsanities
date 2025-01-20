using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrantEffectController : EffectController
{
    protected GrantEffectObject grantEffectObject => (GrantEffectObject) effectObject;
    
    protected override void OnEffectStarted()
    {
        // Start the effect or perform any necessary setup
        if(grantEffectObject.items == null || grantEffectObject.items.Length == 0)
        {
            Debug.Log("Null");
            return;
        }
        for (int i = 0; i < grantEffectObject.items.Length; i++) 
        {
            GameObject drop = ((IInventoryObject)grantEffectObject.items[i]).GetDroppedGameObject(grantEffectObject.nums[i], transform.position);
        }
    }
}
