using UnityEngine;


public class DumbController : EnemyController
{
    private new Rigidbody2D rb;
    private float patrolTime = 0f;
    private bool patrolDirection = false;
    private float fleeTime = 5f;
    private float CurrentHP;
    private float PrevHP;
    private bool isFleeing = false;
    private float patrolRest = 2f;
    private float hittingback = 0.3f;

    private new Animator animator;


    // Start is called before the first frame update
    protected override string IdleAnimationState { get; }
    protected override string AttackAnimationState { get; }
    protected override string MoveAnimationState { get; }

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        groundCheckCenter = transform.Find("groundCheckCenter");
        frontCheck = transform.Find("frontCheck");
        backCheck = transform.Find("backCheck");
    }
    protected override void Start()
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
            
            if (patrolDirection)    // walk to right side
            {
                if (MoveForwardDepthCheck())
                {
                    rb.linearVelocity = new Vector2(enemyStats.movingSpeed, rb.linearVelocity.y);
                    if (!facingRight) { Flip(); Debug.Log("dumb idle walk Turn right");}
                }
            }
            else                    // walk to left side
            {
                if (MoveForwardDepthCheck())
                {
                    rb.linearVelocity = new Vector2(-enemyStats.movingSpeed, rb.linearVelocity.y);
                    if (facingRight) { Flip(); Debug.Log("dumb idle walk Turn left");}
                }
            }
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
                    rb.linearVelocity = new Vector2(currentStats.movingSpeed * -2, rb.linearVelocity.y);
                    if (facingRight) { Flip(); }
                }
            }
            else
            {
                if (MoveForwardDepthCheck())
                {
                    rb.linearVelocity = new Vector2(currentStats.movingSpeed * 2, rb.linearVelocity.y);
                    if (!facingRight) { Flip(); }
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
}
