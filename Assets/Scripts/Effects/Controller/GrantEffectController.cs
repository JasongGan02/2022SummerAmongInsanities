using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrantEffectController : EffectController
{
    protected BaseObject[] items;
    protected int[] nums;
    protected override void OnEffectStarted()
    {
        // Start the effect or perform any necessary setup
        if(items == null || items.Length == 0)
        {
            Debug.Log("Null");
            return;
        }
        for (int i = 0; i < items.Length; i++) 
        {
            GameObject drop = ((IInventoryObject)items[i]).GetDroppedGameObject(nums[i], transform.position);
        }
    }
}
