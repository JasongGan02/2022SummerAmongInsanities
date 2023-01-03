using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Villager: Enemy
{
    // Start is called before the first frame update
    void Start()
    {
        //Attack();
        Move();
        ItemsDrop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Attack(Transform target_transform)
    {
        print("Attacking");
    }

    protected override void Move()
    {
        print("Moving");
    }

    protected override void ItemsDrop()
    {
        print("Items Dropping");
    }
}
