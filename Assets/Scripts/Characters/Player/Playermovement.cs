using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playermovement : MonoBehaviour
{
    public float runSpeed;
    public float runSpeedmodifier=2f;
    public float jumpHeight = 7f;
    public float  excavateCoeff = 1f;
   

    public int totalJumps;
    int availableJumps;

    private Rigidbody2D rb;
    private Animator animator;

    public Transform groundCheckLeft;
    public Transform groundCheckCenter;
    public Transform groundCheckRight;
    public LayerMask groundLayer;

    public bool facingRight = true;

    private bool isRunning = false;
    private bool isGrounded = true;
    bool multipleJump;                                      

    public float range = 0.05f;
                                                            
    void Awake()
    {
        availableJumps = totalJumps;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!PlayerStatusRepository.GetIsViewingUi())
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
                isRunning = true;
            if (Input.GetKeyUp(KeyCode.LeftShift))
                isRunning = false;

            if (Input.GetButtonDown("Jump"))
            {
                Jump();
            }
        }

        animator.SetFloat("yVelocity", rb.velocity.y);
    }

    void FixedUpdate()
    {
        if (!PlayerStatusRepository.GetIsViewingUi())
        {
            Movement();
        } else
        {
            ClearHorizontalVelocity();
        }
        CheckCollsionForJump();
    }

    private void ClearHorizontalVelocity()
    {
        rb.velocity = new Vector2(0f, rb.velocity.y);
        animator.SetFloat(Constants.Animator.SPEED, 0);
    }

    public void Movement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal") * runSpeed *excavateCoeff;
        
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
            
            multipleJump = true;
            availableJumps--;

            rb.velocity = new Vector2(rb.velocity.x, 1 * jumpHeight);
            
            

        }
        else
        {
           

            if (multipleJump && availableJumps>0)
            {
                availableJumps--;
                
                rb.velocity = new Vector2(rb.velocity.x, 1 * jumpHeight);
                animator.Play("playerDoubleJump");
                
            }
        }
    }
    private void CheckCollsionForJump()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;
        RaycastHit2D hitLeft = Physics2D.Raycast(groundCheckLeft.position, Vector2.down, range, groundLayer);
        RaycastHit2D hitCenter = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, range, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(groundCheckRight.position, Vector2.down, range, groundLayer);

        if ((hitLeft.transform != null)
            || (hitRight.transform != null)
            || (hitCenter.transform != null))
        {
            isGrounded = true;
             if (!wasGrounded)
            {
                availableJumps = totalJumps;
                multipleJump = false;

            }        
        }
        animator.SetBool("Jump", !isGrounded);
        


    }

     void Flip()
    {
        facingRight = !facingRight;
        
        Vector3 transformScale = transform.localScale;
        transformScale.x *= -1;
        transform.localScale = transformScale;
    }
}