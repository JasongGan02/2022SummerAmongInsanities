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

    public override void ExecuteEffect(IEffectableObject effectedGameObject) //Use this when you are unsure about what type of controller will be using.
    {
        string controllerName = "GrantEffectController";
        Type type = Type.GetType(controllerName);
        var controller = (effectedGameObject as MonoBehaviour).gameObject.AddComponent(type);
        (controller as EffectController).Initialize(this);
    }
}
