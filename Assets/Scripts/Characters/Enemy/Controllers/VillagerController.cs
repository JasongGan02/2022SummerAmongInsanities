using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class VillagerController : EnemyController
{
    bool rest = false;
    float cooldown = 0;
    bool facingright = false;
    private Rigidbody2D rb;
    float patroltime = 0;
    private Animator animator;


    new void Awake()
    {
        animator = GetComponent<Animator>();
        towerContainer = FindObjectOfType<TowerContainer>();
    }

    protected override void EnemyLoop()
    {
        rb = GetComponent<Rigidbody2D>();
        //Debug.Log(Vector2.Distance(transform.position, player.transform.position));
        if (IsPlayerSensed())
        {
            animator.SetBool("IsStanding", true);           // villager stand
            rb.velocity = new Vector2(0, rb.velocity.y);    //  stop patrol
            if (IsPlayerInAtkRange())
            {
                //Debug.Log(timer);
                attack();
                //Debug.Log("attack");
            }else
            {
                // approaching player
                transform.position = Vector2.MoveTowards(transform.position, player.transform.position, MovingSpeed * Time.deltaTime);
                flip(player.transform);
                //Debug.Log("approach");
            }
        }
        else if(IsTowerSensed())
        {
            animator.SetBool("IsStanding", true);           // villager stand
            rb.velocity = new Vector2(0, rb.velocity.y);    // stop patrol
            //Debug.Log("attack tower");
            if (IsTowerInAtkRange( (int) AtkRange))
            {
                // atk tower
                //print("atking tower");
                flip(NearestTowerTransform);
                attackTower(NearestTowerTransform);
            }else
            {
                // approaching tower
                transform.position = Vector2.MoveTowards(transform.position, NearestTowerTransform.position, MovingSpeed * Time.deltaTime);
                flip(NearestTowerTransform);
            }
        }
        else
        {
            animator.SetBool("IsStanding", false);           // villager sit
            patrol();
        }

        SenseFrontBlock();
    }

    

    bool IsTowerInAtkRange(int AtkRange)
    {
        float distance = CalculateDistanceFromEnemyToTower(NearestTowerTransform);
        if (distance <= AtkRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void attackTower(Transform target)
    {
        if (rest)
        {
            if (cooldown > AtkInterval)
            {
                cooldown = 0;
                rest = false;
            }
            else { cooldown += Time.deltaTime; }
            //Debug.Log("rest");
        }

        if (Vector2.Distance(transform.position, NearestTowerTransform.position) > 0.7f)
        {
            transform.position = Vector2.MoveTowards(transform.position, NearestTowerTransform.position, MovingSpeed * 2 * Time.deltaTime);
        }

        if (Vector2.Distance(transform.position, NearestTowerTransform.position) < 1f && !rest)
        {
            StartCoroutine(rotateZombie());
            //Debug.Log("hit");
            NearestTowerTransform.GetComponent<TowerHealth>().DecreaseHealth((int) AtkDamage);
            rest = true;
        }

        flip(NearestTowerTransform.transform);
    }

    // rotate when attack
    IEnumerator rotateZombie()      
    {
        transform.Rotate(0, 0, 30);
        yield return new WaitForSeconds(0.2f);
        transform.Rotate(0, 0, -30);
    }

    new void attack()
    {
        if (rest)
        {
            if (cooldown > AtkInterval)
            {
                cooldown = 0;
                rest = false;
            }
            else { cooldown += Time.deltaTime; }
           //Debug.Log("rest");
        }

        if (Vector2.Distance(transform.position, player.transform.position) > 0.7f)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, MovingSpeed * 2 * Time.deltaTime);
        }
        
        if (Vector2.Distance(transform.position, player.transform.position) < 1f && !rest)
        {
            StartCoroutine(rotateZombie());
            //Debug.Log("hit");
            player.GetComponent<PlayerController>().takenDamage(AtkDamage);
            rest = true;
        }

        flip(player.transform);
    }

    void patrol()
    {
        if (patroltime <= 0f)
        {
            patroltime = Random.Range(1, 5);
            if (Random.Range(0f, 1f) > 0.5) // go left
            {
                rb.velocity = new Vector2(-MovingSpeed, rb.velocity.y);
                //Debug.Log("going to left");
                if (facingright) { flip(); }
            }
            else                          // go right
            {
                rb.velocity = new Vector2(MovingSpeed, rb.velocity.y);
                if (!facingright) { flip(); }
                //Debug.Log("going to right");
            }
        }
        else
        {
            patroltime -= Time.deltaTime;
        }
    }

    void flip(Transform target)
    {
        if (target.position.x >= transform.position.x && !facingright)
        {
            facingright = true;
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (target.position.x < transform.position.x && facingright)
        {
            facingright = false;
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }
    void flip()
    {
        if (facingright)
        {
            facingright = false;
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            facingright = true;
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    public override void death()
    {
        Destroy(this.gameObject);
        DropItems();
    }

    

    void DropItems()
    {
        
    }
}
