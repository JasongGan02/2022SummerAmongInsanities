using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotionController : MedicineController
{
    private bool isHealing = false;
    private float healTimeCount = 0f;
   

    // Update is called once per frame
    public override void Update()
    {
        //Flip();

        if (Input.GetMouseButton(0))
        {
            UseItem();
        }
        else
        {
            PatrolItem();
        }
    }

    public override void UseItem()
    {

        if (player == null)
        {
            Debug.Log("player is null");
            try { player = GameObject.FindGameObjectWithTag("Player"); }
            catch { Debug.Log("Player is dead or missing"); }
        }
        

        if (player != null)
        {
            Debug.Log(HealAmount);
            player.GetComponent<PlayerController>().Heal(2);
        }

    }


}
