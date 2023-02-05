using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] List<GameObject> enemy_prefabs;
    EnemyContainer enemyContainer;

    float timer;
    // Start is called before the first frame update
    void Start()
    {
        enemyContainer = FindObjectOfType<EnemyContainer>();
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer+=Time.deltaTime;
        if(timer >= 3 && !enemyContainer.is_too_much())   // Nathan's change
        {
            GeneratingEnemy(0);
            timer = 0;
        }
    }

    void GeneratingEnemy(int enemy_type)
    {
        GameObject instance = Instantiate(enemy_prefabs[enemy_type], transform.position, Quaternion.identity);
        Transform parent_transform = enemyContainer.transform;
        instance.transform.SetParent(parent_transform);
    }

}
