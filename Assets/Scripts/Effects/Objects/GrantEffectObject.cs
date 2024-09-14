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
    
    
}
