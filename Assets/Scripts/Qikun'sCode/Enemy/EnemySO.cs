using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "ScriptableObjects/EnemyScriptableObject", order = 1)]
public class EnemySO : ScriptableObject
{
    public string EnemyName;
    public float HP;
    public float AtkDamage;
    public float AtkInterval;
    public float MovingSpeed;
    public float SensingRange;
    public Drop[] drops;
}
