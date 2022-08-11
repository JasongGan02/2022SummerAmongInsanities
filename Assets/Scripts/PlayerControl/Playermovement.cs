using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playermovement : MonoBehaviour
{
    public float runSpeed;
    public float runSpeedmodifier=2f;
    public float jumpHeight = 7f;


    private Rigidbody2D rb;
    private Animator animator;

    public Transform groundCheck;
    public LayerMask groundLayer;

    private bool facingRight = true;
    private bool isRunning = false;
    private bool isGrounded = true;
    public Vector3 range;
                                                            


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();


    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift))
            isRunning = true;
        if(Input.GetKeyUp(KeyCode.LeftShift))
            isRunning = false;

        if(Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        animator.SetFloat("yVelocity", rb.velocity.y);
    }

    void FixedUpdate()
    {
        
        Movement();
        CheckCollsionForJump();
        
        
    }


    private void Movement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal") * runSpeed;
        
        if(isRunning)
            moveInput*=runSpeedmodifier;
        animator.SetFloat(Constants.Animator.SPEED, Mathf.Abs(moveInput));
        //speed: 0 idle, 3.5 walking, 7 running

        rb.velocity = new Vector2(moveInput, rb.velocity.y);

       
            
        if(moveInput > 0 && !facingRight || moveInput < 0 && facingRight)
            Flip();
    }
    private void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 1 * jumpHeight);
        }
    }
    private void CheckCollsionForJump()
    {
        isGrounded = false;
        Collider2D hit = Physics2D.OverlapBox(groundCheck.position, range, 0, groundLayer);

        if(hit != null && hit.gameObject.tag == Constants.Tag.GROUND)
        {
            isGrounded = true;
            
        } 
        animator.SetBool("Jump", !isGrounded);
    }



    private void Flip()
    {
        facingRight = !facingRight;
        
        Vector3 transformScale = transform.localScale;
        transformScale.x *= -1;
        transform.localScale = transformScale;
    }

}
