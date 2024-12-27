using System;
using System.Collections;
using UnityEngine;

public class SoulController : PickupController
{
    private float experienceValue;
    private PlayerExperience playerExperience;

    public void Initialize(float experienceValue)
    {
        player =  player = GameObject.FindGameObjectWithTag("Player");
        playerExperience = player.GetComponent<PlayerExperience>();
        this.experienceValue = experienceValue;
        UpdateChunk();
    }
    
    protected override void OnPickup()
    {
        if (playerExperience == null)
        {
            Destroy(gameObject);
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
