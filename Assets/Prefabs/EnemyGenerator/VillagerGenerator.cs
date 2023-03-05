using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class VillagerGenerator : CharacterSpawner
{
    public int EnemyMax;
    private float timerr;

    void Start()
    {
        timerr = 0f;
    }

    protected new void Update()
    {
        timerr += Time.deltaTime;
        if (!TooMuch(EnemyMax) && timerr > 3f)
        {
            SpawnVillager();
            timerr = 0f;
        }
    }

}
