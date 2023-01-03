using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//目前维护者：Jason 
public abstract class CharacterPanel: MonoBehaviour
{
    //战斗系统vars
    [Header("Character Basic Attributes") ]
    [SerializeField]
    public float maxHealth;
    [SerializeField]
    public float curHealth;
    [SerializeField]
    public float speed;
    [SerializeField]
    public float atk_damage;
    [SerializeField]
    public int armor;
    [SerializeField]
    public int magic_resist;
    [SerializeField]
    public float atk_speed; //每秒攻击几次

    [SerializeField]
    public Drop[] drops;

    
    void Start()
    {
        curHealth = maxHealth;
    }

    void Update()
    {

    }


    public virtual void takenDamage(float damage)
    {
        curHealth = Mathf.Max(curHealth-damage, 0);
        if(curHealth == 0)
            Death();

    }

    protected abstract void Death(); //怪物的进pool，人物的特殊情况，防御塔的也进pool。


}
