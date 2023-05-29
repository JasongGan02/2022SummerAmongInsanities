using System;
using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        CurrentHP = HP;
        PrevHP = CurrentHP;
    }

    // Update is called once per frame
    protected override void EnemyLoop(){
        CurrentHP = HP;
        Debug.Log("PrevHP " + PrevHP);
        Debug.Log("CurrentHP " + CurrentHP);
        if (PrevHP > CurrentHP || isFleeing)
        {
            isFleeing = true;
            flee();
            PrevHP = CurrentHP;
            Debug.Log("Flee");
        }
        else if(HP <= maxHP/2){
            Vector2 direction = (player.transform.position - transform.position);
            if (direction.x > 0)
            {
                transform.right = Vector2.left;
            }
            else if (direction.x < 0)
            {
                transform.right = Vector2.right;
            }
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, -MovingSpeed * Time.deltaTime);
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
            patrolTime = UnityEngine.Random.Range(1f, 3f);
            if (UnityEngine.Random.Range(0f,1f) > 0.5f)
            {
                patrolDirection = true;
            }
            else { patrolDirection = false; }
        }
        else
        {
            patrolTime -= Time.deltaTime;
            if (patrolDirection)
            {
                rb.velocity = new Vector2(MovingSpeed, rb.velocity.y);
                if (!facingRight) { flip(); }
            }
            else
            {
                rb.velocity = new Vector2(-MovingSpeed, rb.velocity.y);
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
        if (fleeTime > 0f)
        {
            fleeTime -= Time.deltaTime;
            if (player.transform.position.x > transform.position.x)
            {
                rb.velocity = new Vector2(MovingSpeed * -2, rb.velocity.y);
                if (facingRight) { flip(); }
            }
            else
            {
                rb.velocity = new Vector2(MovingSpeed * 2, rb.velocity.y);
                if (!facingRight) { flip(); }
            }
        }
        else { 
            fleeTime = 5f;
            isFleeing = false;
        }
    }
}
