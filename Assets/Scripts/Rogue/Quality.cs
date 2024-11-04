using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quality 
{
    public ItemRarity rarity;
    public Color color;
    public float weight;
    public Sprite sprite;
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
