using System.Collections;
using UnityEngine;

// This class represents an archer tower
public class ArcherTowerController : AttackTowerController
{
    GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        transform.parent = GameObject.Find("TowerContainer").transform; // Nathan's change
        enemyContainer = FindObjectOfType<EnemyContainer>();
        isEnemySpotted = false;
        InvokeRepeating("Attack", 0f, AtkInterval);
        //Hatred.Add("BatController");
        //Hatred.Add("VillagerController");
        //Hatred.Add("LadyController");
    }

    // Shoot a bullet at the nearest enemy
    public void Attack()
    {
        //Transform enemyTransform = SenseNearestEnemyTransform();
        //if (enemyTransform == null || !isEnemySpotted) return;

        //if (bullet != null)
        //{
        //    Vector3 direction = enemyTransform.position - transform.position;
        //    GameObject bulletInstance = Instantiate(bullet, transform.position, Quaternion.identity);
        //    bulletInstance.transform.SetParent(transform);
        //    bulletInstance.GetComponent<Rigidbody2D>().velocity = direction * bullet_speed;
        //}

        target = WhatToAttack();
        if (target != null && bullet != null)
        {
            Vector3 direction = target.transform.position - transform.position;
            GameObject bulletInstance = Instantiate(bullet, transform.position, Quaternion.identity);
            bulletInstance.transform.SetParent(transform);
            bulletInstance.GetComponent<Rigidbody2D>().velocity = direction * bullet_speed;
        }
        else { return; }
    }


}