using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public GameObject player;
    public GameObject target;
    public GameObject arrow;

    public float speed;

    private float nextFire; // the time at which the archer can fire again
    private bool canFire = true; // flag to check if the archer can fire
    public float fireRate; // rate at which the archer fires arrows
    public bool canDetect = true; // flag to check if the archer can detect
    public bool canChangeMod = true; // flag to check if the attackMod can change
    public float chnageModInterval = 2f;
    public float nextModChange;


    public Vector3 movePosition;

    private float playerX;
    private float playerY;
    private float targetX;
    private float targetY;
    private float nextX;
    private float dist;
    private float baseY;
    private float height;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Lady");
        target = GameObject.Find("Player");
        arrow = GameObject.Find("Arrow");
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(player.transform.position, target.transform.position) <= 4 && canDetect)
        {
            speed = 3.5f;
            fireRate = 1.5f;
            canDetect = false;
            
        }else if (Vector2.Distance(player.transform.position, target.transform.position) > 4 && canDetect){
            speed = 7f;
            fireRate = 3f;
            canDetect = false;
        }
        if (canFire)
        {
                // Fire an arrow
            playerX = player.transform.position.x;
            playerY = player.transform.position.y;
            targetX = target.transform.position.x;
            targetY = target.transform.position.y - 0.2f;
            dist = targetX - playerX;
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


        if (Vector2.Distance(player.transform.position, target.transform.position) <= 4)
        {
            attackMod_2();

        }
        else if (Vector2.Distance(player.transform.position, target.transform.position) > 4)
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

    public static Quaternion LookAtTarget(Vector2 r)
    {
        return Quaternion.Euler(0, 0, Mathf.Atan2(r.y, r.x) * Mathf.Rad2Deg);
    }

    void attackMod_1()
        {
            nextX = Mathf.MoveTowards(transform.position.x, targetX, speed * Time.deltaTime);
            baseY = playerY;

            movePosition = new Vector2(nextX, baseY + 0.15f);
            transform.position = movePosition;
        }

    void attackMod_2()
        {
            nextX = Mathf.MoveTowards(transform.position.x, targetX, speed * Time.deltaTime);
            baseY = Mathf.Lerp(player.transform.position.y, targetY, (nextX - playerX) / dist);
            height = 1.25f * (nextX - playerX) * (nextX - targetX) / (-0.25f * dist * dist);

            movePosition = new Vector3(nextX, baseY + height, transform.position.z);

            transform.rotation = LookAtTarget(movePosition - transform.position);
            transform.position = movePosition;
        }
}