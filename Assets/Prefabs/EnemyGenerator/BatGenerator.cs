using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BatGenerator : CharacterSpawner
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
            SpawnBat();
            timerr = 0f;
        }
    }

}