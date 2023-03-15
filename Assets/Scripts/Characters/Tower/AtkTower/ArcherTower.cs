using System.Collections;
using UnityEngine;

// This class represents an archer tower
public class ArcherTower : TowerController
{
    // Start is called before the first frame update
    void Start()
    {
        enemyContainer = FindObjectOfType<EnemyContainer>();
        isEnemySpotted = false;
    }

    // Update is called once per frame
    void Update()
    {
        Transform enemyTransform = SenseNearestEnemyTransform();
        if (isEnemySpotted)
        {
            if (Time.time >= AtkTimer)
            {
                Shoot(enemyTransform);
                AtkTimer = Time.time + AtkInterval;
            }
        }
    }

    // Shoot a bullet at the nearest enemy
    void Shoot(Transform enemyTransform)
    {
        if (enemyTransform == null) return;

        if (bullet != null)
        {
            Vector3 direction = enemyTransform.position - transform.position;
            GameObject bulletInstance = Instantiate(bullet, transform.position, Quaternion.identity);
            bulletInstance.GetComponent<Rigidbody2D>().velocity = direction * bullet_speed;
        }
    }
}