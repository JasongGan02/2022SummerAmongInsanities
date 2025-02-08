using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneDaggerController : Weapon

{
    protected override void FollowPlayerWithFloat()
    {
        idleBobTime += Time.deltaTime * idleBobSpeed;
        float bobOffset = Mathf.Sin(idleBobTime) * idleBobAmount;

        Vector3 targetPosition = player.transform.position + idleOffset + new Vector3(0, bobOffset, 0);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);


        if (targetEnemy != null)
        {
            Vector2 directionToEnemy = (targetEnemy.position - player.transform.position).normalized;
            transform.rotation = Quaternion.Euler(0, 0, directionToEnemy.x < 0 ? 90 : 270);
            transform.localScale = new Vector3(directionToEnemy.x < 0 ? -0.33f : 0.33f, 0.33f, 0.33f);
        }
        else
        {
            if (playerMovement != null)
            {
                transform.rotation = Quaternion.Euler(0, 0, !playerMovement.facingRight ? 90 : 270);
                transform.localScale = new Vector3(!playerMovement.facingRight ? -0.33f : 0.33f, 0.33f, 0.33f);
            }
        }
    }


}
