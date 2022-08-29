using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherTower : MonoBehaviour
{
    [SerializeField] float AtkRange;        // When enemy enters the atk range, the tower will start fire
    [SerializeField] float AtkSpeed;        // Atk Speed affects the Atk Timer counting. When AtkSpeed = 1, the time elapsed speed = real time elapsed speed
    [SerializeField] float AtkIntervalTime; // During AtkIntervalTime, the tower will automatically attack once
    [SerializeField] float AtkTimer;        // Timer
    [SerializeField] float bullet_speed;    // bullet flying speed
    [SerializeField] GameObject bullet;
    
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
        foreach(Transform e in enemyTransforms)
        {
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

    // 计算从敌方到当前塔的距离
    float CalculateDistanceFromEnemyToArcherTower(Transform enemyTransform)
    {
        Vector3 towerPosition = transform.position;
        Vector3 enemyPosition = enemyTransform.position;
        float distance = Mathf.Sqrt(Mathf.Pow((towerPosition.x - enemyPosition.x),2) + Mathf.Pow((towerPosition.y - enemyPosition.y),2));
        return distance;
    }

    // 朝目标敌人发射一颗子弹
    void FireToEnemy(Transform enemyTransform)
    {
        // 计算向量
        Vector3 direction = enemyTransform.position - transform.position;
        // 朝向量方向发射子弹
        GameObject bullet_instance = Instantiate(bullet, transform.position, Quaternion.identity);
        // Give bullet speed
        bullet_instance.GetComponent<Rigidbody2D>().velocity = direction * bullet_speed;
    }
}
