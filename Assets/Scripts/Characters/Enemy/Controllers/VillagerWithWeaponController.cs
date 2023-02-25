using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VillagerWithWeaponController : EnemyController
{
    private GameObject target;
    public Transform Player;
    private Vector3 movePosition;
    private float speed = 2.5f;
    private float nextJump;
    private bool canJump;

    // Start is called before the first frame update
    void Start()
    {
        AtkDamage = 5f;
        player = GameObject.FindGameObjectWithTag("Player");
        Player = player.transform;
        target = GameObject.FindGameObjectWithTag("tower"); //临时

    }

    // Update is called once per frame
    void Update()
    {
        EnemyLoop();
    }

    protected override void EnemyLoop()
    {
        if (canJump)
        {
            SenseFrontBlock();
            nextJump = Time.time + 5f;
        }
        if (Time.time > nextJump)
        {
            canJump = true;
        }
        
        if(IsTowerSensed()){
            AtkDamage = 10f;
            Vector2 direction = (target.transform.position - transform.position);
            if (direction.x > 0)
            {
                transform.right = Vector2.left;
            }
            else if (direction.x < 0)
            {
                transform.right = Vector2.right;
            }
            target = GameObject.FindGameObjectWithTag("tower");
            transform.position = Vector2.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);


        } 
        else if (IsPlayerSensed()){
            AtkDamage = 5f;
            Vector2 direction = (Player.position - transform.position);
            if (direction.x > 0)
            {
                transform.right = Vector2.left;
            }
            else if (direction.x < 0)
            {
                transform.right = Vector2.right;
            }

            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);


        }
    }


}
