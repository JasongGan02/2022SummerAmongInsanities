using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyObject : CharacterObject
{
    public float SensingRange;
    
    public void LevelUp()
    {
        _HP *= 1.1f;
        _atkDamage *= 1.1f;
    }
}
