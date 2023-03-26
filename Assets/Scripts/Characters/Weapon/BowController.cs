using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BowController : Weapon
{

    public override void attack()
    {
       
    }


    public override void Patrol()
    {
        if (playermovement.facingRight)
        {
            transform.position = player.transform.position + Vector3.right * 0.8f;


        }
        else
        {
            transform.position = player.transform.position - Vector3.right * 0.8f;


        }
    }


}
