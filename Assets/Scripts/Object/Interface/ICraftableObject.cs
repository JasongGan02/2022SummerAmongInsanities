﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICraftableObject
{

    BaseObject[] Recipe { get; set; }

    int[] Quantity { get; set; }

    bool IsCraftable { get; set; }

    bool IsCoreNeeded { get; set; }

    int CraftTime { get; set; }

    GameObject GetDroppedGameObject(int amount, Vector3 dropPosition);
    public void Craft(Inventory inventory);

    public BaseObject[] getRecipe();

    public int[] getQuantity();

    public bool getIsCraftable();

    public bool getIsCoreNeeded();

    public int getCraftTime();

}