using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is Catapult tower
public class CatapultTower : MonoBehaviour
{
    [SerializeField] float AtkRange;        // When enemy enters the atk range, the tower will start fire
    [SerializeField] float AtkSpeed;        // Atk Speed affects the Atk Timer counting. When AtkSpeed = 1, the time elapsed speed = real time elapsed speed
    [SerializeField] float AtkIntervalTime; // During AtkIntervalTime, the tower will automatically attack once
    [SerializeField] float bullet_x_flyingSpeed;   // Bullet flying speed in x axis, absolute value without direction
    [SerializeField] GameObject bullet;
    float AtkTimer;        // Timer
    bool isEnemySpotted;
    EnemyContainer enemyContainer;
    // Start is called before the first frame update
    void Start()
    {
        enemyContainer = FindObjectOfType<EnemyContainer>();
        isEnemySpotted = false;
        AtkTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        Transform enemyTransform = SenseEnemyTransform();
        if(isEnemySpotted)
        {
            AtkTimer += Time.deltaTime * AtkSpeed;
            if(AtkTimer >= AtkIntervalTime)
            {
                FireToEnemy(enemyTransform);
                AtkTimer = 0f;
            }
            
        }
    }

    // 目前是在数组里找到第一个进入玩家范围的敌人
    // 后续更新：找到距离玩家最近的敌人
    Transform SenseEnemyTransform()
    {
        Transform[] enemyTransforms = enemyContainer.GetComponentsInChildren<Transform>();
        // print(enemyTransforms[0]);
        foreach(Transform e in enemyTransforms)
        {
            if(e==enemyTransforms[0])
            {
                continue;
            }
            float dist = CalculateDistanceFromEnemyToArcherTower(e);
            // Find enemy
            if(dist <= AtkRange)
            {
                isEnemySpotted = true;
                return e;
            }
        }

        // No enemy detected
        isEnemySpotted = false;
        return transform;
    }

    // Calculate the distance between enemy and current tower
    float CalculateDistanceFromEnemyToArcherTower(Transform enemyTransform)
    {
        Vector3 towerPosition = transform.position;
        Vector3 enemyPosition = enemyTransform.position;
        float distance = Mathf.Sqrt(Mathf.Pow((towerPosition.x - enemyPosition.x),2) + Mathf.Pow((towerPosition.y - enemyPosition.y),2));
        return distance;
    }

    // Shoot a bullet to enemy transform
    void FireToEnemy(Transform enemyTransform)
    {
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
