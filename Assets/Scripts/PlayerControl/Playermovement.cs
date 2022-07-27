using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playermovement : MonoBehaviour
{
    public float runSpeed, jumpForce;
    private float jumpHeight = .4f;

    private Rigidbody2D playerRigidbody;
    private Animator animator;

    public Transform groundCheck;
    public LayerMask groundLayer;

    private bool facingRight = true;

    public Vector3 range;
                                                            


    void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        Application.targetFrameRate = 50;

    }

    
    void FixedUpdate()
    {
        Movement();
        CheckCollsionForJump();
        
        
    }

    private void Movement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal") * runSpeed;

        animator.SetFloat(Constants.Animator.SPEED, Mathf.Abs(moveInput));


        playerRigidbody.velocity = new Vector2(moveInput, playerRigidbody.velocity.y);

        if(Input.GetKeyUp(KeyCode.Space))
        {
            if(playerRigidbody.velocity.y > 0)
            {
                playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x,playerRigidbody.velocity.y * jumpHeight);
            }
        }

        if(moveInput > 0 && !facingRight || moveInput < 0 && facingRight)
            Flip();
    }

    private void CheckCollsionForJump()
    {
        Collider2D hit = Physics2D.OverlapBox(groundCheck.position, range, 0, groundLayer);

        if(hit != null && hit.gameObject.tag == Constants.Tag.GROUND)
        {
            animator.SetBool(Constants.Animator.IN_AIR, false);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, jumpForce);
            }
        } 
        else
        {
            animator.SetBool(Constants.Animator.IN_AIR, true);

        }
    }



    private void Flip()
    {
        facingRight = !facingRight;
        
        Vector3 transformScale = transform.localScale;
        transformScale.x *= -1;
        transform.localScale = transformScale;
    }

}
