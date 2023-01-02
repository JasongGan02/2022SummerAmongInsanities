using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentAtlas", menuName = "Atlas/Equipment Atlas")]
public class EquipmentAltas : ScriptableObject
{
    [Header("Weapons")]
    public WeaponObject axe;
    public WeaponObject arrow;
    public WeaponObject bow;
    public WeaponObject hand;
    public WeaponObject shovel;
    public WeaponObject spear;
    public WeaponObject stoneDagger;
}
