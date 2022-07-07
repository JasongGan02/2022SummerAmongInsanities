using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playermovement : MonoBehaviour
{
    public float runSpeed, jumpForce;
    private float jumpHeight = .4f;
    private float moveInput;

    private Rigidbody2D myBody;
    private Animator anim;

    public Transform groundCheck;
    public LayerMask groundLayer;

    private bool facingRight = true;

    public Vector3 range;
                                                            


    void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        Application.targetFrameRate = 50;

    }

    
    void FixedUpdate()
    {
        Movement();
        CheckCollsionForJump();
        
        
    }

    void Movement()
    {
        moveInput = Input.GetAxisRaw("Horizontal") * runSpeed;

        anim.SetFloat("speed", Mathf.Abs(moveInput));


        myBody.velocity = new Vector2(moveInput, myBody.velocity.y);

        if(Input.GetKeyUp(KeyCode.Space))
        {
            if(myBody.velocity.y > 0)
            {
                myBody.velocity = new Vector2(myBody.velocity.x,myBody.velocity.y * jumpHeight);
            }
        }

        if(moveInput > 0 && !facingRight || moveInput < 0 && facingRight)
            Flip();
    }

    void CheckCollsionForJump()
    {
        Collider2D hit = Physics2D.OverlapBox(groundCheck.position, range, 0, groundLayer);

        if(hit != null)
        {
            if(hit.gameObject.tag == "ground" && Input.GetKeyDown(KeyCode.Space)) 
            {
                myBody.velocity = new Vector2(myBody.velocity.x, jumpForce);
                anim.SetBool("jump", true);
            }
            else 
            {
                anim.SetBool("jump", false);
            }
        }
    }



    void Flip()
    {
        facingRight = !facingRight;
        
        Vector3 transformScale = transform.localScale;
        transformScale.x *= -1;
        transform.localScale = transformScale;
    }

}
