using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(menuName = "Effects/Tower Upgrades/MultipleShotsArcherTowerEffectObject")]
public class MultipleShotsArcherTowerEffectObject : EffectObject
{
    [Header("MultipleShotsArcherTowerEffectObject Fields")]
    public int projectilesPerShot = 1;
    
    public override async void InitializeEffectObject()
    {
        // Load the target tower object
        var archerTower = await AddressablesManager.Instance.LoadAssetAsync<RangedTowerObject>("Assets/ScriptableObjects/CharacterObjects/TowerObject/ArcherTower.asset");
        archerTower.rangedTowerStats.projectilesPerShot += projectilesPerShot;

    }
}
