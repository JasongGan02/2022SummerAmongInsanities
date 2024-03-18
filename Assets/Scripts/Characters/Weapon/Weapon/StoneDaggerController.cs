using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneDaggerController : Weapon

{
  
    public override void attack()
    {
       
        if (Input.GetMouseButtonDown(0))
            InvokeRepeating("PlayAttack", 0.1f, 1f);

        float speed = maxSpeed; // Set the default speed to the maximum speed
        transform.Rotate(Vector3.forward * 2000f * Time.deltaTime);

        if (playermovement.facingRight)
        {


            transform.position = player.transform.position + new Vector3(1f, 0, 0) + Vector3.right * Mathf.Sin(Time.time * frequency) * magnitude * speed;

        }
        else
        {
            transform.position = player.transform.position - new Vector3(1f, 0, 0) + Vector3.right * Mathf.Sin(Time.time * frequency) * magnitude * speed;

        }
        

    }



}
