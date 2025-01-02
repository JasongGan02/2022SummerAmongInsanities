using System;
using System.Collections;
using UnityEngine;

public class SoulController : PickupController
{
    private float experienceValue;
    private PlayerExperience playerExperience;

    public void Initialize(float experienceValue)
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerExperience = player.GetComponent<PlayerExperience>();
        }
        this.experienceValue = experienceValue;
        UpdateChunk();
        NormalizeObjectSize();
    }

    protected override void FindFields()
    {
       base.FindFields();
        if (player != null)
            playerExperience = player.GetComponent<PlayerExperience>();
    }
    
    protected override void OnPickup()
    {
        if (playerExperience == null)
        {
            Destroy(gameObject);
            return;
        }
        playerExperience.AddExperience(experienceValue);
        Destroy(gameObject);
        player.GetComponent<CharacterController>().GetAudioEmitter().PlayClipFromCategory("ItemPickUp");
    }

    protected override void HandleOutOfBound()
    {
        Destroy(gameObject);
    }

    protected override bool CheckBeforePickingUp()
    {
        return playerExperience != null;
    }
}
