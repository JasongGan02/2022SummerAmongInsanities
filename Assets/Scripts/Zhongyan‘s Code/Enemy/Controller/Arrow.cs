using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : EnemyController
{
    private GameObject lady;
    private GameObject target;
    private LadyController file;
    public float speed;
    private float nextFire; // the time at which the archer can fire again
    private bool canFire = true; // flag to check if the archer can fire
    public float fireRate; // rate at which the archer fires arrows
    public bool canDetect = true; // flag to check if the archer can detect
    public bool canChangeMod = true; // flag to check if the attackMod can change
    public float chnageModInterval = 2f;
    public float nextModChange;


    public Vector3 movePosition;

    private float ladyX;
    private float ladyY;
    private float targetX;
    private float targetY;
    private float nextX;
    private float dist;
    private float baseY;
    private float height;

    // Start is called before the first frame update
    void Start()
    {
        lady = transform.parent.gameObject;
        file = lady.GetComponent<LadyController>();
    }

    // Update is called once per frame
    protected override void EnemyLoop()
    {
        target = file.target;
        if (lady == null || target == null) // the parent gameobject is destroied
        {
            death();
        }

        if (Vector2.Distance(lady.transform.position, target.transform.position) <= 4 && canDetect)
        {
            speed = 3.5f;
            fireRate = 1.5f;
            canDetect = false;
            
        }else if (Vector2.Distance(lady.transform.position, target.transform.position) > 4 && canDetect){
            speed = 7f;
            fireRate = 3f;
            canDetect = false;
        }
        if (canFire)
        {
                // Fire an arrow
            ladyX = lady.transform.position.x;
            ladyY = lady.transform.position.y;
            targetX = target.transform.position.x;
            targetY = target.transform.position.y - 0.2f;
            dist = targetX - ladyX;
            if (target.transform.position.y > 16.3)
            {
                if (target.transform.position.x < transform.position.x)
                {
                    targetX = target.transform.position.x-1;
                    targetY = 16.2f;
                }
                else{
                    targetX = target.transform.position.x+1;
                    targetY = 16.2f;
                }
            }
            

                // Set the next fire time
            nextFire = Time.time + fireRate;

                // Set the canFire flag to false
            canFire = false;
        }

            // Check if the fire rate has passed
        if (Time.time > nextFire)
        {
                // Set the canFire flag to true
            canFire = true;
        }


        if (Vector2.Distance(lady.transform.position, target.transform.position) <= 4)
        {
            attackMod_2();

        }
        else if (Vector2.Distance(lady.transform.position, target.transform.position) > 4)
        {
            attackMod_2();

        }







        if (dist < 0){
            if (transform.position.x + 0.01 < targetX || transform.position.x - 0.01 < targetX)
            {
               if (transform.position.y < 16.3)
               {
                   Destroy(gameObject);
                   canDetect = true;
               }
            }
        }else{
            if (transform.position.x + 0.01 > targetX || transform.position.x - 0.01 > targetX)
            {
               if (transform.position.y < 16.3)
               {
                   Destroy(gameObject);
                   canDetect = true;
               }
            }
        }


    }


    void attackMod_1()
        {
            nextX = Mathf.MoveTowards(transform.position.x, targetX, speed * Time.deltaTime);
            baseY = ladyY;

            movePosition = new Vector2(nextX, baseY + 0.15f);
            transform.position = movePosition;
        }

    void attackMod_2()
        {
            nextX = Mathf.MoveTowards(transform.position.x, targetX, speed * Time.deltaTime);
            baseY = Mathf.Lerp(lady.transform.position.y, targetY, (nextX - ladyX) / dist);
            height = 1.25f * (nextX - ladyX) * (nextX - targetX) / (-0.25f * dist * dist);

            movePosition = new Vector3(nextX, baseY + height, transform.position.z);

            transform.position = movePosition;
        }


}