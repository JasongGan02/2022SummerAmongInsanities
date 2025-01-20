using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Character Objects/Tower Object/Ranged Tower Object")]
public class RangedTowerObject : TowerObject
{
    [Header("RangedTowerObject Fields")]
    public RangedTowerStats rangedTowerStats;
    public ProjectileObject projectileObject;
    
    protected override void OnEnable()
    {
        baseStats = rangedTowerStats;  // Ensure the baseStats is set
        CharacterOnEnable();
    }
}
