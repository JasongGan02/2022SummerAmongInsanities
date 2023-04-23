using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DumbController : EnemyController
{

    int maxHP;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    protected override void EnemyLoop(){
        if(HP <= maxHP/2){
            Vector2 direction = (player.transform.position - transform.position);
            if (direction.x > 0)
            {
                transform.right = Vector2.left;
            }
            else if (direction.x < 0)
            {
                transform.right = Vector2.right;
            }
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, -MovingSpeed * Time.deltaTime);
                //animator.SetFloat("movingSpeed", 1f);
            }
        else{
            idle();
        }
    }

    void idle(){
        Vector3 movePosition = new Vector3(transform.position.x + 5, transform.position.y, 0);
        transform.position = Vector2.MoveTowards(transform.position, movePosition, -MovingSpeed * Time.deltaTime);
    }
}
