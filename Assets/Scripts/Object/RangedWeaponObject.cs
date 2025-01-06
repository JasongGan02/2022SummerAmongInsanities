using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "weapon", menuName = "Objects/Weapon Object/RangedWeaponObject")]
public class RangedWeaponObject : WeaponObject
{
    [Header("RangedWeaponObject Properties")] 
    public ProjectileObject projectileObject;
}
