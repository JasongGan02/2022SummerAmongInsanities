using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.Tilemaps;
using UnityEngine;


public class DumbController : EnemyController
{
    private Rigidbody2D rb;
    private float patrolTime = 0f;
    private bool patrolDirection = false;
    private bool facingRight = false;
    private float fleeTime = 5f;
    private float CurrentHP;
    private float PrevHP;
    private bool isFleeing = false;
    private float patrolRest = 2f;
    private float hittingback = 0.3f;

    private Animator animator;


    public Transform frontCheck;
    public Transform groundCheckCenter;
    public Transform backCheck;
    LayerMask ground_mask;


    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        ground_mask = LayerMask.GetMask("ground");
        groundCheckCenter = transform.Find("groundCheckCenter");
        frontCheck = transform.Find("frontCheck");
        backCheck = transform.Find("backCheck");

    }
    private void Start()
    {
        CurrentHP = currentStats.hp;
        PrevHP = currentStats.hp;
    }

    // Update is called once per frame
    protected override void UpdateEnemyBehavior(){
        CurrentHP = currentStats.hp;
        SenseFrontBlock();
        if (PrevHP > CurrentHP)
        {
            animator.Play("dumb_knockback");
            hittingback = 0.3f;
            isFleeing = true;
            PrevHP = CurrentHP;
        }
        else if (isFleeing)
        {
            flee();
        }
        else if(currentStats.hp <= characterObject.maxStats.hp/2){
            //Debug.Log("HP less than 50%");
            animator.Play("dumb_flee");
            Vector2 direction = (player.transform.position - transform.position);
            if (direction.x > 0)
            {
                transform.right = Vector2.left;
            }
            else if (direction.x < 0)
            {
                transform.right = Vector2.right;
            }
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, -currentStats.movingSpeed * Time.deltaTime);
        }
        else{
            idle();
        }
    }

    void idle(){

        if (patrolTime <= 0f)
        {
            animator.Play("dumb_idle");
            patrolRest = 2f;
            patrolTime = UnityEngine.Random.Range(1f, 3f);
            if (UnityEngine.Random.Range(0f,1f) > 0.5f)
            {
                patrolDirection = true;
            }
            else { patrolDirection = false; }
        }
        else if (patrolRest > 0)
        {
            patrolRest -= Time.deltaTime;
        }
        else
        {
            animator.Play("dumb_walk");
            patrolTime -= Time.deltaTime;
            if (patrolDirection)
            {
                if (MoveForwardDepthCheck())
                {
                    rb.velocity = new Vector2(currentStats.movingSpeed, rb.velocity.y);
                    if (!facingRight) { flip(); }
                }
            }
            else
            {
                if (MoveForwardDepthCheck())
                {
                    rb.velocity = new Vector2(-currentStats.movingSpeed, rb.velocity.y);
                    if (facingRight) { flip(); }
                }
            }
        }
    }
    void flip()
    {
        if (facingRight)
        {
            facingRight = false;
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            facingRight = true;
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }
    void flee()
    {
        if (hittingback > 0f) { hittingback -= Time.deltaTime; }
        else if (fleeTime > 0f)
        {
            animator.Play("dumb_flee");
            fleeTime -= Time.deltaTime;
            if (player.transform.position.x > transform.position.x)
            {
                if (MoveForwardDepthCheck())
                {
                    rb.velocity = new Vector2(currentStats.movingSpeed * -2, rb.velocity.y);
                    if (facingRight) { flip(); }
                }
            }
            else
            {
                if (MoveForwardDepthCheck())
                {
                    rb.velocity = new Vector2(currentStats.movingSpeed * 2, rb.velocity.y);
                    if (!facingRight) { flip(); }
                }
            }
        }
        else {
            animator.Play("dumb_walk");
            hittingback = 0.3f;
            fleeTime = 5f;
            isFleeing = false;
        }
    }

    new void SenseFrontBlock()
    {
        if(MoveForwardDepthCheck() == false) { return; }

        headCheck();
        RaycastHit2D hitCenter = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitFront = Physics2D.Raycast(frontCheck.position, Vector2.left, 0.1f, ground_mask);
        RaycastHit2D hitBack = Physics2D.Raycast(backCheck.position, Vector2.right, 0.1f, ground_mask);

        if (hitCenter.transform != null)
        {
            if ((facingRight && rb.velocity.x > 0) || (!facingRight && rb.velocity.x < 0))
            {
                if (hitFront.transform != null)
                {
                    if (headCheck())
                    {
                        Jump();
                    }
                }
            }
            else if ((facingRight && rb.velocity.x < 0) || (!facingRight && rb.velocity.x > 0))
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
        rb.velocity = new Vector2(rb.velocity.x * 1.0f, currentStats.jumpForce);
    }

    private bool MoveForwardDepthCheck() // when walking forward, don't go to abyss
    {
        Vector2 frontDepthDetector = new Vector2(frontCheck.position.x + 0.35f, frontCheck.position.y);
        RaycastHit2D hit = Physics2D.Raycast(frontDepthDetector, Vector2.down, 3f, ground_mask);
        if (hit.collider != null) { return true; }
        return false;
    }

    protected override void MoveTowards(Transform targetTransform)
    {
        Vector2 direction = (targetTransform.position - transform.position).normalized;
        rb.velocity = direction * currentStats.movingSpeed;
    }
}
