using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHeath : MonoBehaviour
{
    [SerializeField] int health_point;
    
    public void DecreaseHealth(int damage)
    {
        health_point -= damage;

        if(health_point <= 0)
        {
            Destroy(gameObject);
        }
    }

}
