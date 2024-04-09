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
    bool facingright = false;
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
    private float attacking_animation_timer = 0f;
    float damage_start_time_0 = 0.17f;

    protected override void Awake()
    {
        base.Awake();
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
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void EnemyLoop()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("villager_idle") == false)
        { SenseFrontBlock(); ChangeCollider("Stand"); }
        else { ChangeCollider("Sit"); }


        target = WhatToAttack();
        if (target == null) { patrol(); }
        else
        {
            if (villager_sight())
            {
                if (DistanceToTarget(target.transform) < _atkRange)
                {
                    attack(target.transform, 1f / _atkSpeed); // default:1;  lower -> faster
                }
                else
                {
                    approach(2.0f * _movingSpeed, target.transform);
                    flip(target.transform);
                }
            }
        }
        ShakePlayerOverHead();
    }

    void attack(Transform target, float frequency)
    {
        // start attack
        if (!rest && attacking_animation_timer <= 0)
        {
            animator.Play("villager_attack");
            attacking_animation_timer = 0.25f; // Time & Speed of animation
        }

        // wait for attack behavior finish
        else if (attacking_animation_timer > 0) // make sure the attack behavior animation is complete
        {
            attacking_animation_timer -= Time.deltaTime;

            if (attacking_animation_timer < (0.25f - damage_start_time_0 + 0.03f) && attacking_animation_timer > (0.25f - damage_start_time_0 - 0.01f))
            {
                float checkD = Vector2.Distance(attackEnd.position, player.transform.position);
                if (checkD < 0.75f) // hurt target successfully
                {
                    ApplyDamage(target.GetComponent<CharacterController>());
                }
                rest = true;
                attacking_animation_timer = 0f;
                Wait = frequency;
            }

        }

        // finished attack and wait for next, this else if should be changed to else later!
        else if (rest)
        {
            if (Wait > 0)
            {
                Wait -= Time.deltaTime;
                animator.Play("villager_rest");
            }
            else
            {
                rest = false;
            }
        }

        flip(target);
    }
    void approach(float speed, Transform target)
    {
        if (speed > _movingSpeed)
        {
            animator.Play("villager_run");
        }
        else
        {
            animator.Play("villager_walk");
        }
        if (MoveForwardDepthCheck() == false) { return; }
        if (target.position.x > transform.position.x) { rb.velocity = new Vector2(speed, rb.velocity.y); }
        else { rb.velocity = new Vector2(-speed, rb.velocity.y); }
    }

    void patrol()
    {
        if (patroltime <= 0f)
        {
            patrolRest = 2f;
            animator.Play("villager_idle");
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
            animator.Play("villager_walk");
            patroltime -= Time.deltaTime;
            if (patrolToRight)
            {
                if (MoveForwardDepthCheck() == true) 
                {
                    rb.velocity = new Vector2(_movingSpeed, rb.velocity.y);
                    if (!facingright) { flip(); }
                }
            }
            else
            {
                if (MoveForwardDepthCheck() == true)
                {
                    rb.velocity = new Vector2(-_movingSpeed, rb.velocity.y);
                    if (facingright) { flip(); }
                }
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
        if (MoveForwardDepthCheck() == false) { return; }
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
    private bool MoveForwardDepthCheck() // when walking forward, don't go to abyss
    {
        Vector2 frontDepthDetector = new Vector2(frontCheck.position.x + 0.35f, frontCheck.position.y);
        RaycastHit2D hit = Physics2D.Raycast(frontDepthDetector, Vector2.down, 3f, ground_mask);
        if (hit.collider != null) { return true; }
        return false;
    }

    new void ShakePlayerOverHead()
    {
        if (System.Math.Abs(player.transform.position.x - transform.position.x) < 0.3f)
        {
            if (System.Math.Abs(player.transform.position.y - transform.position.y) < 2f)
            {
                Debug.Log("shaking the player over head");
                if (Random.Range(0f, 1f) <= 0.5f)
                {
                    rb.velocity = new Vector2(-12f, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(12f, rb.velocity.y);
                }
            }
        }
    }
}
