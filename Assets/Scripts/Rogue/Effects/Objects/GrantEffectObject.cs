using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(menuName = "Effects/Grant")]
public class GrantEffectObject : EffectObject
{
    [Header ("Inventory Object to be granted")]
    public BaseObject[] items; // Assuming InventoryObjectBase is a base class for your inventory items
    public int[] nums;

    // You can then cast this to IInventoryObject where you need to use it
    //public IInventoryObject[] Items => items as IInventoryObject[];
    
}
