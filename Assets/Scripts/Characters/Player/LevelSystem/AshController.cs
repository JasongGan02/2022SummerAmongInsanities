using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AshController : PickupController
{
    private float ashValue;
    private PlayerExperience playerExperience;
    
    public void Initialize(float ashValue)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerExperience = player.GetComponent<PlayerExperience>();
        this.ashValue = ashValue;
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
        }
        playerExperience.AddAsh(ashValue);
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
