using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "weapon", menuName = "Scriptable Object/Weapon")]
public class WeaponScriptableObject : BaseItem
{
    public int damage;
    public float attackFrequency;
}


