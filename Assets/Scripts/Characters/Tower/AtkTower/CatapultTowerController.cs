using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is Catapult tower
public class CatapultTowerController : TowerController
{
    [SerializeField] float bullet_x_flyingSpeed;   // Bullet flying speed in x axis, absolute value without direction
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        enemyContainer = FindObjectOfType<EnemyContainer>();
        animator = GetComponent<Animator>();
        isEnemySpotted = false;
        InvokeRepeating("Attack", 0f, AtkInterval);
    }

    

    // Update is called once per frame
    void Update()
    {
    }

    void Attack()
    {
        Transform enemyTransform = SenseNearestEnemyTransform();
        Debug.Log("11");
        if (isEnemySpotted)
        {
            Shoot(enemyTransform);
        }
    }

    // Shoot a bullet to enemy transform
    /*
    void Shoot(Transform enemyTransform)
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
*/
    Vector2 CalculateBulletSpeed(Transform enemyTransform)
    {
        float deltaX = enemyTransform.position.x - transform.position.x;
        if(Mathf.Abs(deltaX) <= 0.01f)
        {
            Debug.Log("Catapult cannot firing in this direction");
            return Vector2.zero;
        }

        float bullet_xSpeed = (deltaX > 0) ? bullet_x_flyingSpeed : -bullet_x_flyingSpeed;

        float deltaY = enemyTransform.position.y - transform.position.y;
        float flying_time = deltaX / bullet_xSpeed;
        float gravity = Physics2D.gravity.y;
        float bullet_ySpeed = (deltaY - 0.5f * gravity * flying_time * flying_time) / flying_time;

        return new Vector2(bullet_xSpeed, bullet_ySpeed);
    }

    IEnumerator ResetRotation()
    {
        yield return new WaitForSeconds(0.5f);
        transform.rotation = Quaternion.identity;
    }

    void Shoot(Transform enemyTransform)
    {
        Vector2 bullet_speed = CalculateBulletSpeed(enemyTransform);
        if (bullet_speed == Vector2.zero)
        {
            return;
        }

        animator.Play("Catapult_Attack", -1, 0f);

        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, enemyTransform.position - transform.position);
        transform.rotation = rotation;

        GameObject bullet_instance = Instantiate(bullet, transform.position, Quaternion.identity);
        bullet_instance.GetComponent<Rigidbody2D>().velocity = bullet_speed;

        StartCoroutine(ResetRotation());
    }
}