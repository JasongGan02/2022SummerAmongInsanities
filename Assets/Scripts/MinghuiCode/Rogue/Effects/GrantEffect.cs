using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrantEffect : Effect
{
    public IInventoryObject item;
    public GameObject SpawnSource;
    public override void ExecuteEffect(IEffectable character)
    {
        if (character is PlayerController)
        {
            PlayerController player = (PlayerController)character;
            //player.inventory.AddItem(item, 1);
        }
    }
}
