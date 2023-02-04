using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is Catapult tower
public class CatapultTower : TowerBasics
{
    [SerializeField] float bullet_x_flyingSpeed;   // Bullet flying speed in x axis, absolute value without direction
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        enemyContainer = FindObjectOfType<EnemyContainer>();
        animator = GetComponent<Animator>();
        isEnemySpotted = false;
        AtkTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        Transform enemyTransform = SenseNearestEnemyTransform();
        if(isEnemySpotted)
        {
            AtkTimer += Time.deltaTime;
            if(AtkTimer >= AtkIntervalTime)
            {
                animator.Play("Catapult_Attack",-1,0f);
                FireToEnemy(enemyTransform);
                AtkTimer = 0f;
            }
        }
    }

    // Shoot a bullet to enemy transform
    void FireToEnemy(Transform enemyTransform)
    {
        // rotate transform
        if(enemyTransform.position.x>transform.position.x)
        {
            transform.eulerAngles = new Vector3(0,0,0);
        }else
        {
            transform.eulerAngles = new Vector3(0,180,0);
        }
        // Consider the direction of shooting bullet
        float deltaX = enemyTransform.position.x - transform.position.x;
        if(Mathf.Abs(deltaX)<= 0.01f)
        {
            Debug.Log("Catapult cannot firing in this direction");
            return;
        }
        float bullet_xSpeed = 0.0f;
        if(deltaX < 0)
        {
            bullet_xSpeed  = -bullet_x_flyingSpeed;
        }else{
            bullet_xSpeed = bullet_x_flyingSpeed;
        }
        
        float deltaY = enemyTransform.position.y - transform.position.y;
        float flying_time = deltaX / bullet_xSpeed;
        float gravity = Physics2D.gravity.y;
        float bullet_ySpeed = (deltaY - 0.5f*gravity*flying_time*flying_time)/flying_time;

        Vector2 bullet_speed = new Vector2(bullet_xSpeed, bullet_ySpeed);
        
        // Shooting the bullet in calculated direction
        GameObject bullet_instance = Instantiate(bullet, transform.position, Quaternion.identity);
        bullet_instance.GetComponent<Rigidbody2D>().velocity = bullet_speed;
    }
}
