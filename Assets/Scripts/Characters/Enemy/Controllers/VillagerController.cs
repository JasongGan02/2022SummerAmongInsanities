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
    public Transform backCheck;
    public Transform attackStart;
    public Transform attackEnd;
    LayerMask ground_mask;

    private BoxCollider2D boxCollider;

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
        backCheck = transform.Find("backCheck");
        attackStart = transform.Find("attackStart");
        attackEnd = transform.Find("attackEnd");
        boxCollider = GetComponent<BoxCollider2D>();

        //Hatred.Add("PlayerController");
        //Hatred.Add("ArcherTowerController");
        //Hatred.Add("CatapultTowerController");
        //Hatred.Add("TrapTowerController");
    }

    protected override void EnemyLoop()
    {
        
        //UpdateNearestTower();
        rb = GetComponent<Rigidbody2D>();
        Debug.Log("velocity:" + rb.velocity.x + ", " + rb.velocity.y);
        if (animator.GetBool("IsStanding") == true) { SenseFrontBlock(); ChangeCollider("Stand"); }
        else { ChangeCollider("Sit"); }
        

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
                approachPlayer(2 * _movingSpeed);
                flip(player.transform);
            }
        }
        else
        {
            if (IsTowerInAtkRange((int)_atkRange) && villager_sight())
            {
                flip(target.transform);
                attackTower(target.transform);
            }
            else
            {
                animator.SetBool("IsStanding", true);
                animator.SetBool("IsRunning", false);
                approach(_movingSpeed, target.transform);
                flip(target.transform);
            }
        }
    }

    bool IsTowerInAtkRange(int AtkRange)
    {
        if (target != null)
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
        return false;
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
                if (cooldown > _atkSpeed)
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
            approach(2 * _movingSpeed, target);
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
                if (cooldown > _atkSpeed)
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
            approachPlayer(2 * _movingSpeed);
        }
        
        else 
        {
            animator.SetBool("IsStanding", true);
            animator.SetBool("IsRunning", true);
            animator.SetBool("Attack", true);
            //Debug.Log("hit");

            float checkD = Vector2.Distance(attackEnd.position, player.transform.position);
            Debug.Log("distance: " + checkD);
            if (checkD < 0.15f)
            {
                player.GetComponent<PlayerController>().takenDamage(AtkDamage);
                rest = true;
                Wait = 0.3f;
            }
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
                rb.velocity = new Vector2(_movingSpeed, rb.velocity.y);
                if (!facingright) { flip(); }
            }
            else
            {
                rb.velocity = new Vector2(-_movingSpeed, rb.velocity.y);
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
        RaycastHit2D hitFront = Physics2D.Raycast(frontCheck.position, Vector2.left, 0.1f, ground_mask);
        RaycastHit2D hitBack = Physics2D.Raycast(backCheck.position, Vector2.right, 0.1f, ground_mask);

        if (hitCenter.transform != null)
        {
            if ((facingright && rb.velocity.x > 0) || (!facingright && rb.velocity.x < 0))
            {
                if (hitFront.transform != null)
                {
                    if (headCheck())
                    {
                        Jump();
                    }
                }
            }
            else if ((facingright && rb.velocity.x < 0) || (!facingright && rb.velocity.x > 0))
            {
                if (hitBack.transform != null)
                {
                    if (headCheck())
                    {
                        Jump();
                    }
                }
            }
        }

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
        rb.velocity = new Vector2(rb.velocity.x * 1.0f, _jumpForce);
    }
    private bool villager_sight()
    {
        Rigidbody2D targetRB = target.GetComponent<Rigidbody2D>();
        Vector2 targetTop = targetRB.position + Vector2.up * GetComponent<Collider2D>().bounds.extents.y;
        Vector2 villagerTop = rb.position + Vector2.up * GetComponent<Collider2D>().bounds.extents.y;
        Vector2 targetBottom = targetRB.position + Vector2.down * GetComponent<Collider2D>().bounds.extents.y;
        Vector2 villagerBottom = rb.position + Vector2.down * GetComponent<Collider2D>().bounds.extents.y;

        Debug.DrawRay(targetTop, villagerTop - targetTop, Color.red);   // top
        Debug.DrawRay(targetBottom, villagerBottom - targetBottom, Color.red);   // bottom

        float distance1 = Vector2.Distance(targetTop, villagerTop);
        float distance2 = Vector2.Distance(targetBottom, villagerBottom);

        RaycastHit2D checkTop = Physics2D.Raycast(targetTop, villagerTop - targetTop, distance1, ground_mask);
        RaycastHit2D checkBottom = Physics2D.Raycast(targetBottom, villagerBottom - targetBottom, distance2, ground_mask);
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
    public void ChangeCollider(string status)
    {
        // Enable or disable the colliders based on the state
        // boxCollider.enabled = !isSitting;
        if (status == "Stand")
        {
            boxCollider.size = new Vector2(0.1875544f, 1.0f);
        }
        else
        {
            boxCollider.size = new Vector2(0.1875544f, 0.718245f);
        }
    }
}
