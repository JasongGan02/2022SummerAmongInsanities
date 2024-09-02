using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(menuName = "Effects/Upgrade")]
public class CharUpgradeEffectObject : EffectObject
{
    [Header("Character Upgrade Effect Settings")]
    public CharacterStats statModifications;
    
}
