using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Scriptable Objects/Character Objects/Player Object")]

public class PlayerObject : CharacterObject
{
    [SerializeField] private PlayerStats playerStats;
    
    protected override void OnEnable()
    {
        baseStats = playerStats;  // Ensure the baseStats is set
        base.OnEnable();
    }
    
    public override List<GameObject> GetDroppedGameObjects(bool playerDropItemsOnDeath, Vector3 dropPosition)
    {
        List<GameObject> droppedItems = new();
        return droppedItems;
    }
}
