using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerHealth : MonoBehaviour
{
    [SerializeField] double health_point;
    
    public void DecreaseHealth(float damage)
    {
        health_point -= damage;

        if(health_point <= 0)
        {
            Destroy(gameObject);
        }
    }
}
