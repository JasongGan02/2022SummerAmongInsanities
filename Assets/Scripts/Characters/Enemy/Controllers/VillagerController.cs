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
    bool patrolToRight = true;
    float patrolRest = 2f;


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
                SenseFrontBlock();
                approachPlayer(MovingSpeed);
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
                SenseFrontBlock();
            }
        }
        else
        {
            SenseFrontBlock();
            patrol();
        }

        
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
            SenseFrontBlock();
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
            approachPlayer(MovingSpeed * 2f);
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

    void approachPlayer(float speed)
    {
        Vector2 target = new Vector2(player.transform.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    void patrol()
    {
        if (patroltime <= 0f)
        {
            patrolRest = 5f;
            animator.SetBool("IsStanding", false);
            patroltime = Random.Range(1f, 3f);
            if (Random.Range(0f, 1f) < 0.5) // go left
            {
                patrolToRight = false;
            }
            else                          // go right
            {
                patrolToRight = true;
            }
        }
        else if (patrolRest > 0)
        {
            patrolRest -= Time.deltaTime;
        }
        else
        {
            animator.SetBool("IsStanding", true);
            patroltime -= Time.deltaTime;
            if (patrolToRight) 
            { 
                rb.velocity = new Vector2(MovingSpeed, rb.velocity.y);
                if (!facingright) { flip(); }
            }
            else
            {
                rb.velocity = new Vector2(-MovingSpeed, rb.velocity.y);
                if (facingright) { flip(); }
            }
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


    new void SenseFrontBlock()
    {
        Vector3 shooting_direction = transform.TransformDirection(-Vector3.right);
        Vector3 origin = transform.position - new Vector3(0, 0.5f, 0);

        LayerMask ground_mask = LayerMask.GetMask("ground");

        float bottomline = 0.3f;
        RaycastHit2D hit = Physics2D.Raycast(origin, shooting_direction, 0.5f, ground_mask);
        Debug.DrawRay(origin, shooting_direction * 0.5f, Color.green); // infront
        Vector3 left = transform.position - new Vector3(0.25f, 0.35f, 0);
        RaycastHit2D bottomLeft = Physics2D.Raycast(left, Vector3.down, bottomline, ground_mask);
        Debug.DrawRay(left, Vector3.down * bottomline, Color.blue);        // bottom left
        Vector3 right = transform.position - new Vector3(-0.25f, 0.35f, 0);
        RaycastHit2D bottomRight = Physics2D.Raycast(right, Vector3.down, bottomline, ground_mask);
        Debug.DrawRay(right, Vector3.down * bottomline, Color.blue);        // bottom right

        if ((hit.collider != null && hit.collider.gameObject.tag == "ground") &&
            ((bottomLeft.collider != null && bottomLeft.collider.gameObject.tag == "ground")||
            (bottomRight.collider != null && bottomRight.collider.gameObject.tag == "ground")))
        {
            //Vector2 up_force = new Vector2(0, JumpForce);
            //gameObject.GetComponent<Rigidbody2D>().AddForce(up_force); 
            //Debug.Log("up_force: " + up_force);

            Vector2 up_force = new Vector2(0, JumpForce);
            Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
            rb.AddForce(up_force, ForceMode2D.Impulse);
            Debug.Log("up_force: " + up_force);
            StartCoroutine(StopJump(rb, 0.7f)); //stop the jump after 0.7 seconds
        }

    }
    IEnumerator StopJump(Rigidbody2D rb, float duration)
    {
        yield return new WaitForSeconds(duration);
        rb.velocity = new Vector2(rb.velocity.x, 0);
    }

    
}
