using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is archer tower part
public class ArcherTower : TowerBasics
{
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
        Transform enemyTransform = SenseNearestEnemyTransform();
        if(isEnemySpotted)
        {
            AtkTimer += Time.deltaTime;
            if(AtkTimer >= AtkIntervalTime)
            {
                FireToEnemy(enemyTransform);
                AtkTimer = 0f;
            }
            
        }
    }

    void FireToEnemy(Transform enemyTransform)
    {
        Vector3 direction = enemyTransform.position - transform.position;
        GameObject bullet_instance = Instantiate(bullet, transform.position, Quaternion.identity);
        bullet_instance.GetComponent<Rigidbody2D>().velocity = direction * bullet_speed;
    }
}
