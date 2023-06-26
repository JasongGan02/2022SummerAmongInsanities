using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICraftableObject
{

    BaseObject[] Recipe { get; set; }

    int[] Quantity { get; set; }

    public void Craft(Inventory inventory);

    public BaseObject[] getRecipe();

    public int[] getQuantity();

}