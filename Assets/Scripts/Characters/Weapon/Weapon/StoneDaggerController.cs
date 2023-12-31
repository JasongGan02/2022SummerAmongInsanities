using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneDaggerController : Weapon

{
  
    public override void attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            am.playAudio(am.attack);
            am.looponAudio();
        }
        else
        {
            am.loopoffAudio();
        }

        if (Input.GetMouseButtonUp(0))
        {
            am.StopPlayerAudio();
        }
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        float speed = maxSpeed; // Set the default speed to the maximum speed
        transform.Rotate(Vector3.forward * 2000f * Time.deltaTime);

        if (playermovement.facingRight)
        {
            if (transform.position.x > player.transform.position.x + slowDownDistance)
            {
                // Gradually slow down as the object approaches the slow down distance
                float percentSlowDown = Mathf.Clamp01((transform.position.x - (player.transform.position.x + slowDownDistance)) / slowDownDistance);
                speed = maxSpeed * (1 - percentSlowDown);
            }

            transform.position = player.transform.position + new Vector3(1f, 0, 0) + Vector3.right * Mathf.Sin(Time.time * frequency) * magnitude * speed;

        }
        else
        {
            transform.position = player.transform.position - new Vector3(1f, 0, 0) + Vector3.right * Mathf.Sin(Time.time * frequency) * magnitude * speed;

        }
        

    }



}
