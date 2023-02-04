using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyContainer : MonoBehaviour
{
    public int maxEnemy;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Nathan's addition
    public bool is_too_much()
    {
        if (GameObject.FindGameObjectsWithTag("enemy").Length > maxEnemy) 
        {
            return true; 
        }
        return false;
    }
}
