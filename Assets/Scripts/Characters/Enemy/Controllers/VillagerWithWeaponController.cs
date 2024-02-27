using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class VillagerWithWeaponController : EnemyController
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
    LayerMask ground_mask;
    
    private float Wait = 0.3f;
    private float horizontal_speed;

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

    }

    protected override void EnemyLoop()
    {

        //UpdateNearestTower();
        rb = GetComponent<Rigidbody2D>();
        horizontal_speed = rb.velocity.x;

        if (horizontal_speed > _movingSpeed) { animator.SetBool("walk", true); animator.SetBool("run", true); }
        else if (horizontal_speed > 0f) { animator.SetBool("walk", true); animator.SetBool("run", false); }

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
                approachPlayer(1.3f * _movingSpeed);
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
                animator.SetBool("attack", false);
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
            animator.SetBool("attack", false);
            approach(2 * _movingSpeed, target);
        }

        else
        {
            animator.SetBool("attack", true);
            ApplyDamage(target.GetComponent<CharacterController>());
            rest = true;
            Wait = 0.2f;
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
                animator.SetBool("attack", false);
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
            animator.SetBool("attack", false);
            approachPlayer(2 * _movingSpeed);
        }

        else
        {
            animator.SetBool("attack", true);
            //Debug.Log("hit");
            ApplyDamage(player.GetComponent<CharacterController>());
            rest = true;
            Wait = 0.2f;
        }

        flip(player.transform);
    }
    void approachPlayer(float speed)
    {
        if (player.transform.position.x > transform.position.x) { rb.velocity = new Vector2(speed, rb.velocity.y); }
        else { rb.velocity = new Vector2(-speed, rb.velocity.y); }
    }
    void approach(float speed, Transform target)
    {
        if (target.position.x > transform.position.x) { rb.velocity = new Vector2(speed, rb.velocity.y); }
        else { rb.velocity = new Vector2(-speed, rb.velocity.y); }
    }
    void patrol()
    {
        animator.SetBool("attack", false);
        if (patroltime <= 0f)
        {
            patrolRest = 2f;
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

        if (hitLeft.transform != null
            || hitRight.transform != null
            || hitCenter.transform != null)
        {
            if (hitFront.transform != null || hitBack.transform != null)
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
        Vector2 jumpForce = new Vector2(rb.velocity.x, _jumpForce);
        rb.AddForce(jumpForce, (ForceMode2D)ForceMode.Impulse);
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
}
