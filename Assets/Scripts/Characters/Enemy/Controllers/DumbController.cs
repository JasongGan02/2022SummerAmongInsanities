using UnityEngine;


public class DumbController : EnemyController
{
    private new Rigidbody2D rb;
    private float PatrolTime = 0f;
    private bool PatrolDirection = false;
    private float FleeTime = 5f;
    private float CurrentHP;
    private float PrevHP;
    private bool IsFleeing = false;
    private float PatrolRest = 2f;
    private float HittingBack = 0.3f;

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
            HittingBack = 0.3f;
            IsFleeing = true;
            PrevHP = CurrentHP;
        }
        else if (IsFleeing)
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

        if (PatrolTime <= 0f)
        {
            animator.Play("dumb_idle");
            PatrolRest = 2f;
            PatrolTime = UnityEngine.Random.Range(1f, 3f);
            if (UnityEngine.Random.Range(0f,1f) > 0.5f)
            {
                PatrolDirection = true;
            }
            else { PatrolDirection = false; }
        }
        else if (PatrolRest > 0)
        {
            PatrolRest -= Time.deltaTime;
        }
        else
        {
            animator.Play("dumb_walk");
            PatrolTime -= Time.deltaTime;
            
            if (PatrolDirection)    // walk to right side
            {
                if (MoveForwardDepthCheck())
                {
                    rb.linearVelocity = new Vector2(enemyStats.movingSpeed, rb.linearVelocity.y);
                    if (!facingRight) { Flip(); }
                }
            }
            else                    // walk to left side
            {
                if (MoveForwardDepthCheck())
                {
                    rb.linearVelocity = new Vector2(-enemyStats.movingSpeed, rb.linearVelocity.y);
                    if (facingRight) { Flip();}
                }
            }
        }
    }
    void flee()
    {
        if (HittingBack > 0f) { HittingBack -= Time.deltaTime; }
        else if (FleeTime > 0f)
        {
            animator.Play("dumb_flee");
            FleeTime -= Time.deltaTime;
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
            HittingBack = 0.3f;
            FleeTime = 5f;
            IsFleeing = false;
        }
    }
}
