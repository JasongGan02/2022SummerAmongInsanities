    using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour, IAudioable
{
    [SerializeField]
    private float ms;
    [SerializeField]
    private float runningModifier = 2f;
    [SerializeField]
    private float jumpForce = 7f;
    [SerializeField] 
    public float excavateCoeff = 1f;
    [SerializeField] private float slowDownFactor = 0.1f;

    [SerializeField]
    private float glideSpeed = 5f;

    private bool isGliding = false;
    private float glideActivationDelay = 0.2f;
    private float glideTimer = 0;

    public int totalJumps;
    int availableJumps;

    private Rigidbody2D rb;
    private Animator animator;
    private AudioEmitter _audioEmitter;

    public Transform groundCheckLeft;
    public Transform groundCheckCenter;
    public Transform groundCheckRight;
    public LayerMask groundLayer;

    public bool facingRight = true;
    private bool isInvokingFootsteps = false;
    float tolerance = 0.01f;
    float interval = 0;
    private float walkFootstepInterval = 0.75f; 
    private float runFootstepInterval = 0.4f; 

    private bool isRunning = false;
    private bool isGrounded = true;
    bool multipleJump;                                      

    public float range = 0.05f;
    

    void Awake()
    {
        availableJumps = totalJumps;
        _audioEmitter = GetComponent<AudioEmitter>();
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

        if (availableJumps == 0 && !isGrounded)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                if (!isGliding)
                {
                    glideTimer += Time.deltaTime;
                    if (glideTimer >= glideActivationDelay)
                    {
                        StartGliding();
                        glideTimer = 0;
                    }
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            glideTimer = 0;
            if (isGliding)
            {
                StopGliding();
            }
        }
        animator.SetFloat("yVelocity", rb.velocity.y);
    }

    void FixedUpdate()
    {
        if (!PlayerStatusRepository.GetIsViewingUi())
        {
            if (isGliding) 
            {
                Gliding();
            }
            else
            {
                Movement();
            }
        } 
        else
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
        float moveInput = Input.GetAxisRaw("Horizontal") * ms * excavateCoeff;
        Vector2 direction = facingRight == true ? Vector2.right : Vector2.left;
        RaycastHit2D frontEnemyCheck = Physics2D.Raycast(transform.position, direction, 0.7f, LayerMask.GetMask("enemy"));
        Debug.DrawLine(transform.position, transform.position + (Vector3)direction * 0.7f, Color.red);

        if (frontEnemyCheck.collider != null)
        {
            moveInput *= 0.1f;   
        }
        
        if(isRunning)
            moveInput*=runningModifier;
        animator.SetFloat(Constants.Animator.SPEED, Mathf.Abs(moveInput));
        //speed: 0 idle, 3.5 walking, 7 running

        rb.velocity = new Vector2(moveInput, rb.velocity.y);

        if ((moveInput > 0 && !facingRight || moveInput < 0 && facingRight))
            Flip();

        if (Mathf.Abs(moveInput) > 0 && rb.velocity.y == 0)
        {
            float newInterval = isRunning ? runFootstepInterval : walkFootstepInterval;

            // Check if footstep interval needs to be updated
            if (!isInvokingFootsteps || Mathf.Abs(interval - newInterval) > tolerance)
            {
                CancelInvoke("PlayFootstep");
                interval = newInterval;
                InvokeRepeating("PlayFootstep", 0.1f, interval);
                isInvokingFootsteps = true;

                
            }
        }
        else
        {
            if (isInvokingFootsteps)
            {
                CancelInvoke("PlayFootstep");
                isInvokingFootsteps = false;
                
            }
        }
    }

    void PlayFootstep()
    {
        _audioEmitter.PlayClipFromCategory("PlayerSteps");
    }

    private void Jump()
    {
        if (isGrounded)
        {
            
            multipleJump = true;
            availableJumps--;

            rb.velocity = new Vector2(rb.velocity.x, 1 * jumpForce);
            animator.Play("playerJump");
            _audioEmitter.PlayClipFromCategory("Jump");


        }
        else
        {
           

            if (multipleJump && availableJumps>0)
            {
                availableJumps--;
                
                rb.velocity = new Vector2(rb.velocity.x, 1 * jumpForce);
                animator.Play("playerDoubleJump");
                _audioEmitter.PlayClipFromCategory("DoubleJump");
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

                if (isGliding)
                {
                    StopGliding();
                }

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

    public void StatsChange(float MS, float jumpForce, float totalJumps)
    {
        ms = MS;
        this.jumpForce = jumpForce;
        this.totalJumps = (int) totalJumps;
    }

    public AudioEmitter GetAudioEmitter()
    {
        return _audioEmitter;
    }

    private void StartGliding()
    {
        isGliding = true;
        rb.velocity = new Vector2(rb.velocity.x, 0);
    }

    private void StopGliding()
    {
        isGliding = false;
    }

    private void Gliding()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput != 0)
        {
            if (moveInput > 0 && !facingRight || moveInput < 0 && facingRight)
            {
                Flip();
            }
            facingRight = moveInput > 0;
        }
        rb.velocity = new Vector2(glideSpeed * moveInput, rb.velocity.y * 0.5f);
    }
}
