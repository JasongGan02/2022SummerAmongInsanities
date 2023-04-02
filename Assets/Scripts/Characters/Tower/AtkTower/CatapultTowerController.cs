using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is Catapult tower
public class CatapultTowerController : TowerController
{
    [SerializeField] float bullet_x_flyingSpeed;   // Bullet flying speed in x axis, absolute value without direction
    [SerializeField] float minBulletYSpeed = 5f;  // minimum bullet y speed
    [SerializeField] float maxBulletYSpeed = 10f; // maximum bullet y speed

    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        enemyContainer = FindObjectOfType<EnemyContainer>();
        animator = GetComponent<Animator>();
        isEnemySpotted = false;
        bullet_x_flyingSpeed = bullet_speed;
        InvokeRepeating("Attack", 0.5f, AtkInterval);
    }   

    

    // Update is called once per frame
    void Update()
    {
    }

    public void Attack()
    {
        Transform enemyTransform = SenseNearestEnemyTransform();
        
        if (isEnemySpotted)
        {
            
            Shoot(enemyTransform);
        }
    }

    void flip(Transform enemyTransform)
    {
        if (enemyTransform.position.x < transform.position.x) // Enemy is to the left
        {
            transform.rotation = Quaternion.Euler(0, 180, 0); // Flip 180 degrees
        }
        else // Enemy is to the right
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); // Do not flip
        }
    }

    Vector2 CalculateBulletSpeed(Transform enemyTransform)
    {
        float deltaX = enemyTransform.position.x - transform.position.x;
        if(Mathf.Abs(deltaX) <= 2f)
        {
            Debug.Log("Catapult cannot firing in this direction");
            return Vector2.zero;
        }

        float bullet_xSpeed = (deltaX > 0) ? bullet_x_flyingSpeed : -bullet_x_flyingSpeed;

        float deltaY = enemyTransform.position.y - transform.position.y;
        float flying_time = deltaX / bullet_xSpeed;
        float gravity = Physics2D.gravity.y;
        //float bullet_ySpeed =  (deltaY - 0.5f * gravity * flying_time * flying_time) / flying_time;

        // Use a piecewise function to adjust the y speed based on distance
        float bullet_ySpeed = 0f;
        if (Mathf.Abs(deltaX) <= 5f)
        {
            bullet_ySpeed = 5f;
        }
        else if (Mathf.Abs(deltaX) <= 10f)
        {
            bullet_ySpeed = 10f;
        }
        else
        {
            bullet_ySpeed = (deltaY - 0.5f * gravity * Mathf.Pow(Mathf.Abs(deltaX) / bullet_xSpeed, 2f)) / (Mathf.Abs(deltaX) / bullet_xSpeed);
        }
        Debug.Log(bullet_ySpeed);
        //bullet_ySpeed = Mathf.Clamp(bullet_ySpeed, minBulletYSpeed, maxBulletYSpeed);
        return new Vector2(bullet_xSpeed, bullet_ySpeed);
    }

    void Shoot(Transform enemyTransform)
    {
        Vector2 bullet_speed = CalculateBulletSpeed(enemyTransform);
        if (bullet_speed == Vector2.zero)
        {
            return;
        }

        animator.Play("Catapult_Attack", -1, 0f);
        flip(enemyTransform);
        GameObject bullet_instance = Instantiate(bullet, transform.Find("FiringPoint").position, Quaternion.identity);
        bullet_instance.GetComponent<Rigidbody2D>().velocity = bullet_speed;

        
    }
}
