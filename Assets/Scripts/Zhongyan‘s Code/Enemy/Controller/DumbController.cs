using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.Tilemaps;
using UnityEngine;


public class DumbController : EnemyController
{

    int maxHP;
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


    public Transform groundCheckLeft;
    public Transform groundCheckCenter;
    public Transform groundCheckRight;
    public Transform frontCheck;
    LayerMask ground_mask;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        CurrentHP = _HP;
        PrevHP = CurrentHP;
        animator = GetComponent<Animator>();
        ground_mask = LayerMask.GetMask("ground");
        groundCheckLeft = transform.Find("groundCheckLeft");
        groundCheckCenter = transform.Find("groundCheckCenter");
        groundCheckRight = transform.Find("groundCheckRight");
        frontCheck = transform.Find("frontCheck");
    }

    // Update is called once per frame
    protected override void EnemyLoop(){
        CurrentHP = _HP;
        SenseFrontBlock();
        if (PrevHP > CurrentHP)
        {
            animator.SetBool("walk", false);
            animator.SetBool("flee", false);
            animator.SetBool("knockback", true);
            hittingback = 0.3f;
            isFleeing = true;
            PrevHP = CurrentHP;
        }
        else if (isFleeing)
        {
            flee();
        }
        else if(_HP <= maxHP/2){
            animator.SetBool("flee", true);
            Vector2 direction = (player.transform.position - transform.position);
            if (direction.x > 0)
            {
                transform.right = Vector2.left;
            }
            else if (direction.x < 0)
            {
                transform.right = Vector2.right;
            }
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, -_movingSpeed * Time.deltaTime);
                //animator.SetFloat("movingSpeed", 1f);
            }
        else{
            idle();
        }
    }

    void idle(){
        //Vector3 movePosition = new Vector3(transform.position.x + 5, transform.position.y, 0);
        //transform.position = Vector2.MoveTowards(transform.position, movePosition, -MovingSpeed * Time.deltaTime);
        

        if (patrolTime <= 0f)
        {
            animator.SetBool("knockback", false);
            animator.SetBool("walk", false);
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
            animator.SetBool("walk", true);
            patrolTime -= Time.deltaTime;
            if (patrolDirection)
            {
                rb.velocity = new Vector2(_movingSpeed, rb.velocity.y);
                if (!facingRight) { flip(); }
            }
            else
            {
                rb.velocity = new Vector2(-_movingSpeed, rb.velocity.y);
                if (facingRight) { flip(); }
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
            animator.SetBool("knockback", false);
            animator.SetBool("flee", true);
            fleeTime -= Time.deltaTime;
            if (player.transform.position.x > transform.position.x)
            {
                rb.velocity = new Vector2(_movingSpeed * -2, rb.velocity.y);
                if (facingRight) { flip(); }
            }
            else
            {
                rb.velocity = new Vector2(_movingSpeed * 2, rb.velocity.y);
                if (!facingRight) { flip(); }
            }
        }
        else {
            animator.SetBool("flee", false);
            hittingback = 0.3f;
            fleeTime = 5f;
            isFleeing = false;
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
                if (headCheck()) { Jump(); Debug.Log("jumping."); }
                else { /*Debug.Log("front obstacle too high!");*/ }
            }
            else { /*Debug.Log("no obstacle in front");*/ }
        }
        else { /*Debug.Log("foot in the air");*/ }

    }
    bool headCheck()
    {
        Vector3 direction = transform.TransformDirection(-Vector3.right);
        Vector3 origin = transform.position + new Vector3(0, -0.4f, 0);
        RaycastHit2D headRay = Physics2D.Raycast(origin, direction, 0.45f, ground_mask);
        Debug.DrawRay(origin, direction * 0.45f, Color.red);        // bottom right
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
    
}
