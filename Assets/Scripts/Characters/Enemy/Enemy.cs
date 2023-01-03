using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy: CharacterPanel
{
    //战斗vars
    [SerializeField]
    protected float find_target_range;
    protected float timer;

    //Find Player Vars
    protected Playermovement player;
    protected bool playerFound = false;

    //Find Tower Vars
    protected bool towerFound = false;
    protected bool FindTower = true; // 这个怪物会不会有塔的仇恨

    //protected CharacterPanel[]
    protected CharacterPanel[] aggroList; //仇恨列表，本质上是个队列，控制怪物对目标的优先级

    void Start()
    {
        player = FindObjectOfType<Playermovement>();
    }

    protected virtual void FindPlayer()
    {
        Vector3 player_position = player.gameObject.transform.position;
        float x1 = transform.position.x;
        float y1 = transform.position.y;
        float x2 = player_position.x;
        float y2 = player_position.y;
        float distance = Mathf.Sqrt((x1-x2)*(x1-x2) + (y1-y2)*(y1-y2));
        playerFound = (distance <= find_target_range);
    }

    protected virtual Transform FindNearestTower()
    {
        return null;
    }

    protected override void Death()
    {
        Destroy(gameObject);
    }
    
    protected virtual void Attack(Transform target_transform)
    {
        timer += Time.deltaTime;
        if(timer >= atk_speed)
        {
            // attack once
            if(target_transform.gameObject.tag == "Player")
            {
                target_transform.GetComponent<PlayerControl>().takenDamage(atk_damage);
            }else if(target_transform.GetComponent<TowerHealth>())
            {
                //target_transform.GetComponent<TowerHealth>().DecreaseHealth(atk_damage);
            }

            timer = 0;
        }
        
    }

    protected abstract void Move();

   
    protected abstract void ItemsDrop();


}
