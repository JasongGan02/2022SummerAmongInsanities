using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Playermovement : MonoBehaviour
{
    [SerializeField]
    private float MS;
    [SerializeField]
    private float runningModifier = 2f;
    [SerializeField]
    private float jumpForce = 7f;
    [SerializeField] 
    public float excavateCoeff = 1f;
   

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

    audioManager am;

    void Awake()
    {
        availableJumps = totalJumps;
        am = GameObject.FindGameObjectWithTag("audio").GetComponent<audioManager>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
    }
    private void Start()
    {
        AnimationClip clip = animator.runtimeAnimatorController.animationClips.FirstOrDefault(c => c.name == "playerDoubleJump");
        if (clip != null)
        {
            clip.events = new AnimationEvent[0];  // Clear all events
        }
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
        float moveInput = Input.GetAxisRaw("Horizontal") * MS *excavateCoeff;
        
        if(isRunning)
            moveInput*=runningModifier;
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

            rb.velocity = new Vector2(rb.velocity.x, 1 * jumpForce);
            am.playAudio(am.jump);


        }
        else
        {
           

            if (multipleJump && availableJumps>0)
            {
                availableJumps--;
                
                rb.velocity = new Vector2(rb.velocity.x, 1 * jumpForce);
                animator.Play("playerDoubleJump");
                am.playAudio(am.doublejump);
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

    public void StatsChange(float MS, float jumpForce, int totalJumps)
    {
        this.MS = MS;
        this.jumpForce = jumpForce;
        this.totalJumps = totalJumps;
    }
   

}
