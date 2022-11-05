using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBasics : MonoBehaviour
{
    string name;
    int HP;
    int atk_damage;
    int physical_resist;
    int magic_resist;
    float atk_rate;
    float move_speed;
    float sense_enemy_range;

    // drop items

    protected abstract void Attack();

    protected abstract void Move();

    protected abstract void EnemyDead();

    protected abstract void ItemsDrop();


}
