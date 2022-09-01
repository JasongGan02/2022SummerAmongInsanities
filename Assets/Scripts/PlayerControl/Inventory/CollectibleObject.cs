using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newCollectibleObject", menuName = "Collectible Object")]
public class CollectibleObject : ScriptableObject
{
    public string itemName;
    public int maxStack;
    public ItemType itemType;
    public float sizeRatio;
    public GameObject droppedItem;  
}

public enum ItemType
{
    Comsumable,
    Equipment,
    Material,
    Misc
}
