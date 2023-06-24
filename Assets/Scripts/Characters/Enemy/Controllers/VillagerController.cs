using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class VillagerController : EnemyController
{
    bool rest = false;
    float cooldown = 0f;
    bool facingright = false;
    private Rigidbody2D rb;
    float patroltime = 0f;
    private Animator animator;
    bool patrolToRight = true;
    float patrolRest = 2f;
    GameObject target;

    public Transform groundCheckLeft;
    public Transform groundCheckCenter;
    public Transform groundCheckRight;
    public Transform frontCheck;
    LayerMask ground_mask;

    private BoxCollider2D boxCollider;
    private EdgeCollider2D edgeCollider;

    private float Wait = 0.3f;

    new void Awake()
    {
        animator = GetComponent<Animator>();
        towerContainer = FindObjectOfType<TowerContainer>();
        ground_mask = LayerMask.GetMask("ground");
        groundCheckLeft = transform.Find("groundCheckLeft");
        groundCheckCenter = transform.Find("groundCheckCenter");
        groundCheckRight = transform.Find("groundCheckRight");
        frontCheck = transform.Find("frontCheck");
        boxCollider = GetComponent<BoxCollider2D>();
        edgeCollider = GetComponent<EdgeCollider2D>();

        Hatred.Add("PlayerController");
        Hatred.Add("ArcherTowerController");
        Hatred.Add("CatapultTowerController");
        Hatred.Add("TrapTowerController");
    }

    protected override void EnemyLoop()
    {
        
        //UpdateNearestTower();
        rb = GetComponent<Rigidbody2D>();

        if (animator.GetBool("IsStanding") == true) { SenseFrontBlock(); ChangeCollider(true); }
        else { ChangeCollider(false); }
        //if (IsPlayerSensed() && villager_sight())   
        //{
        //    if (IsPlayerInAtkRange())
        //    {
        //        attack();
        //    }
        //    else
        //    {
        //        approachPlayer(2 * MovingSpeed);
        //        flip(player.transform);
        //    }
        //}
        //else if(IsTowerSensed())
        //{
        //    if (IsTowerInAtkRange( (int) AtkRange))
        //    {
        //        flip(NearestTowerTransform);
        //        attackTower(NearestTowerTransform); 
        //    }else
        //    {
        //        animator.SetBool("IsStanding", true);
        //        animator.SetBool("IsRunning", false);
        //        transform.position = Vector2.MoveTowards(transform.position, NearestTowerTransform.position, MovingSpeed * Time.deltaTime);
        //        flip(NearestTowerTransform);

        //    }
        //}
        //else
        //{
        //    patrol();
        //}

        target = WhatToAttack();
        if (target == null) { patrol(); }
        else if (target.CompareTag("Player") && IsPlayerSensed() && villager_sight())
        {
            if (IsPlayerInAtkRange())
            {
                attack();
            }
            else
            {
                approachPlayer(2 * MovingSpeed);
                flip(player.transform);
            }
        }
        else
        {
            if (IsTowerInAtkRange((int)AtkRange))
            {
                flip(target.transform);
                attackTower(target.transform);
            }
            else
            {
                animator.SetBool("IsStanding", true);
                animator.SetBool("IsRunning", false);
                approach(MovingSpeed, target.transform);
                flip(target.transform);
            }
        }
    }

    bool IsTowerInAtkRange(int AtkRange)
    {
        float distance = CalculateDistanceFromEnemyToTower(target.transform);
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
            if (Wait > 0) { Wait -= Time.deltaTime; }
            else
            {
                animator.SetBool("IsStanding", true);
                animator.SetBool("IsRunning", false);
                animator.SetBool("Attack", false);
                if (cooldown > AtkInterval)
                {
                    cooldown = 0f;
                    rest = false;
                }
                else { cooldown += Time.deltaTime; }
                //Debug.Log("rest");
            }
        }

        else if (Vector2.Distance(transform.position, target.position) > 1f)
        {
            animator.SetBool("IsStanding", true);
            animator.SetBool("IsRunning", true);
            animator.SetBool("Attack", false);
            approach(2 * MovingSpeed, target);
        }

        else
        {
            animator.SetBool("IsStanding", true);
            animator.SetBool("IsRunning", true);
            animator.SetBool("Attack", true);
            target.gameObject.GetComponent<TowerController>().takenDamage(AtkDamage);
            rest = true;
            Wait = 0.3f;
        }

        flip(target);
    }
    new void attack()
    {
        if (rest)
        {
            if (Wait > 0) { Wait -= Time.deltaTime; }
            else
            {
                animator.SetBool("IsStanding", true);
                animator.SetBool("IsRunning", false);
                animator.SetBool("Attack", false);
                if (cooldown > AtkInterval)
                {
                    cooldown = 0f;
                    rest = false;
                }
                else { cooldown += Time.deltaTime; }
                //Debug.Log("rest");
            }
        }

        else if (Vector2.Distance(transform.position, player.transform.position) > 0.7f)
        {
            animator.SetBool("IsStanding", true);
            animator.SetBool("IsRunning", true);
            animator.SetBool("Attack", false);
            approachPlayer(2 * MovingSpeed);
        }
        
        else 
        {
            animator.SetBool("IsStanding", true);
            animator.SetBool("IsRunning", true);
            animator.SetBool("Attack", true);
            //Debug.Log("hit");
            player.GetComponent<PlayerController>().takenDamage(AtkDamage);
            rest = true;
            Wait = 0.3f;
        }

        flip(player.transform);
    }
    void approachPlayer(float speed)
    {
        animator.SetBool("IsStanding", true);
        if (speed > 1f) { animator.SetBool("IsRunning", true); }
        else { animator.SetBool("IsRunning", false); }
        if (player.transform.position.x > transform.position.x) { rb.velocity = new Vector2(speed, rb.velocity.y); }
        else { rb.velocity = new Vector2(-speed, rb.velocity.y); }
    }
    void approach(float speed, Transform target)
    {
        animator.SetBool("IsStanding", true);
        if (speed > 1f) { animator.SetBool("IsRunning", true); }
        else { animator.SetBool("IsRunning", false); }
        if (target.position.x > transform.position.x) { rb.velocity = new Vector2(speed, rb.velocity.y); }
        else { rb.velocity = new Vector2(-speed, rb.velocity.y); }
    }
    void patrol()
    {
        animator.SetBool("Attack", false);
        if (patroltime <= 0f)
        {
            patrolRest = 2f;
            animator.SetBool("IsStanding", false);
            animator.SetBool("IsRunning", false);
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
            animator.SetBool("IsRunning", false);
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
        headCheck();
        RaycastHit2D hitLeft = Physics2D.Raycast(groundCheckLeft.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitCenter = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitRight = Physics2D.Raycast(groundCheckRight.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitFront = Physics2D.Raycast(frontCheck.position, Vector2.left, 0.05f, ground_mask);
        

        if (hitLeft.transform != null
            || hitRight.transform != null
            || hitCenter.transform != null)
        {
            if (hitFront.transform != null)
            {
                if (headCheck()) { Jump(); /*Debug.Log("jumping."); */ }
                else { /*Debug.Log("front obstacle too high!");*/ }
            }
            else { /*Debug.Log("no obstacle in front");*/ }
        }
        else { /*Debug.Log("foot in the air");*/ }

    }
    bool headCheck()
    {
        Vector3 direction = transform.TransformDirection(-Vector3.right);
        Vector3 origin = transform.position + new Vector3(0, -0.2f, 0);
        RaycastHit2D headRay = Physics2D.Raycast(origin, direction, 0.34f, ground_mask);
        Debug.DrawRay(origin, direction * 0.34f, Color.red);        // bottom right
        if (headRay.collider != null && headRay.collider.gameObject.tag == "ground")
        {
            return false;
        }

        return true;
    }
    private void Jump()
    {
        Vector2 jumpForce = new Vector2(rb.velocity.x, JumpForce);
        rb.AddForce(jumpForce, (ForceMode2D)ForceMode.Impulse);
    }
    private bool villager_sight()
    {
        Rigidbody2D playerRB = player.GetComponent<Rigidbody2D>();
        Vector2 playerTop = playerRB.position + Vector2.up * GetComponent<Collider2D>().bounds.extents.y;
        Vector2 villagerTop = rb.position + Vector2.up * GetComponent<Collider2D>().bounds.extents.y;
        Vector2 playerBottom = playerRB.position + Vector2.down * GetComponent<Collider2D>().bounds.extents.y;
        Vector2 villagerBottom = rb.position + Vector2.down * GetComponent<Collider2D>().bounds.extents.y;

        Debug.DrawRay(playerTop, villagerTop - playerTop, Color.red);   // top
        Debug.DrawRay(playerBottom, villagerBottom - playerBottom, Color.red);   // bottom

        float distance1 = Vector2.Distance(playerTop, villagerTop);
        float distance2 = Vector2.Distance(playerBottom, villagerBottom);

        RaycastHit2D checkTop = Physics2D.Raycast(playerTop, villagerTop - playerTop, distance1, ground_mask);
        RaycastHit2D checkBottom = Physics2D.Raycast(playerBottom, villagerBottom - playerBottom, distance2, ground_mask);
        if (checkTop.collider != null && 
            checkBottom.collider != null &&
            checkTop.collider.gameObject.CompareTag("ground") && 
            checkBottom.collider.gameObject.CompareTag("ground"))
        {
            //Debug.Log("there is ground block");
            return false;
        }
        return true;
    }
    public void ChangeCollider(bool isSitting)
    {
        // Enable or disable the colliders based on the state
        boxCollider.enabled = !isSitting;
        edgeCollider.enabled = isSitting;
    }
}
