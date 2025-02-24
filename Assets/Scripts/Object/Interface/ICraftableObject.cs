using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICraftableObject
{
    CraftRecipe[] Recipe { get; set; }
    
    bool IsCraftable { get; set; }

    bool IsCoreNeeded { get; set; }

    bool IsLocked { get; set; }
    int CraftTime { get; set; }

    GameObject GetDroppedGameObject(int amount, Vector3 dropPosition);
    public void Craft(Inventory inventory);

    public CraftRecipe[] getRecipe();

    public bool getIsCraftable();

    public bool getIsCoreNeeded();

    public bool getIsLocked();

    public int getCraftTime();

}