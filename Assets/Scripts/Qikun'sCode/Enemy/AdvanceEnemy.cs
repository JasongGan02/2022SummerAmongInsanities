using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvanceEnemy : EnemyBasics
{
    // Start is called before the first frame update
    void Start()
    {
        Attack();
        Move();
        EnemyDead();
        ItemsDrop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Attack()
    {
        print("Attacking");
    }

    protected override void Move()
    {
        print("Moving");
    }

    protected override void EnemyDead()
    {
        print("Dead");
    }

    protected override void ItemsDrop()
    {
        print("Items Dropping");
    }
}
