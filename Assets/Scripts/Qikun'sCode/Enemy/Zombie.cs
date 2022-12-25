using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    private Playermovement player;
    private TowerContainer towerContainer;
    private float movingSpeed = 1;

    [SerializeField] int atk_damage;
    [SerializeField] float atk_interval;
    float timer;
    bool isFindTower;
    bool isTouchTower;
    bool isFindPlayer;
    bool isTouchPlayer;
    bool canUseAbility;
    bool is_alpha_reduce;
    int flickering_time = 0;

    Drop[] drops;

    enum Direction {right, left};
    Direction dash_direction;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Playermovement>();
        towerContainer = FindObjectOfType<TowerContainer>();

        timer = 0;

        isFindTower = false;
        isTouchTower = false;
        isFindPlayer = false;
        isTouchPlayer = false;

        canUseAbility = false;
        is_alpha_reduce = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(canUseAbility)
        {
            StartCoroutine("Dash", dash_direction);
            print("dashing");
        }else
        {
            SenseNearestTarget();
        }
    }

    // Dash only effects to tower
    IEnumerator Dash(Direction direction)
    {
        // flicker 5 times
        if(flickering_time <= 5)
        {
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            float color_alpha = 0f;
            float flickering_speed = 2;
            if(is_alpha_reduce)
            {
                color_alpha = spriteRenderer.color.a-0.01f*flickering_speed;
            }else
            {
                color_alpha = spriteRenderer.color.a+0.01f*flickering_speed;
            }

            if(color_alpha>=1 || color_alpha<=0)
            {
                is_alpha_reduce = !is_alpha_reduce;
                flickering_time++;
            }

            spriteRenderer.color = new Color(1,1,1,color_alpha);
            yield return null;
        }else
        {
            if(direction == Direction.right)
            {
                Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
                rb.velocity = new Vector2(3,0);
            }
            if(direction == Direction.left)
            {
                Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
                rb.velocity = new Vector2(-3,0);
            }        
        }

        yield return null;
    }

    // Find nearest player or tower. When begin sense nearest target? 1. Under attack; 2. Find tower or player
    void SenseNearestTarget()
    {
        float distance_to_player = CalculateDistanceToPlayer();
        Transform nearest_tower = FindNearestTower();
        float distance_to_nearest_tower = float.MaxValue;
        if(nearest_tower != transform){
            distance_to_nearest_tower = CalculateDistanceFromEnemyToTower(nearest_tower);
        }

        SensePlayer();

        // dash to nearest tower
        if(distance_to_nearest_tower>=5 && distance_to_nearest_tower<=8)
        {
            canUseAbility = true;
            flickering_time = 0;
            // get direction based on tower transform
            if(nearest_tower.transform.position.x>transform.position.x)
            {
                dash_direction = Direction.right;
            }
            else{
                dash_direction = Direction.left;
            }
        }
        
        if((isFindTower || isFindPlayer) && !isTouchTower && !isTouchPlayer)
        {
            if(distance_to_player <= distance_to_nearest_tower) // consider player part
            {
                if(distance_to_player > 1)
                {
                    // Approaching player
                    ApproachingTarget(player.transform);
                }
            }else
            {
                if(distance_to_nearest_tower > 1)
                {
                    // Approaching tower
                    ApproachingTarget(nearest_tower);
                }
            }
        }

        if(distance_to_nearest_tower <= 1)
        {
            AttackTarget(nearest_tower);
        }else if(distance_to_player <= 1)
        {
            AttackTarget(player.transform);
        }
    }

    void AttackTarget(Transform target_transform)
    {
        timer += Time.deltaTime;
        if(timer >= atk_interval)
        {
            // attack once
            if(target_transform.gameObject.tag == "Player")
            {
                target_transform.GetComponent<PlayerAttributes>().DecreaseHealth(atk_damage);
            }else if(target_transform.GetComponent<TowerHealth>())
            {
                target_transform.GetComponent<TowerHealth>().DecreaseHealth(atk_damage);
            }

            timer = 0;
        }
        
    }


    // sense player's position, if less than certain distance, approaching player
    void SensePlayer()
    {
        float distance = CalculateDistanceToPlayer();

        if(distance <= 15)
        {
            isFindPlayer = true;
        }else{
            isFindPlayer = false;
        }
    }


    // Approaching target with moving speed, this will be complicated when the land becomes complex
    void ApproachingTarget(Transform target_transform)
    {
        transform.position = Vector2.MoveTowards(transform.position, target_transform.position, movingSpeed*Time.deltaTime);
        SenseFrontBlock();
        // transform directiron change
        if(target_transform.position.x >= transform.position.x)
        {
            transform.eulerAngles = new Vector3(0,180,0);
        }else{
            transform.eulerAngles = new Vector3(0,0,0);
        }
    }

    // Shooting a 2D rayline, if sense a ground tag, then jump
    void SenseFrontBlock()
    {
        Vector3 shooting_direction = transform.TransformDirection(-Vector3.right);
        Vector3 origin = transform.position - new Vector3(0,0.3f,0);
        Debug.DrawRay(origin, shooting_direction, Color.green);

        LayerMask ground_mask = LayerMask.GetMask("ground");
        RaycastHit2D hit = Physics2D.Raycast(origin, shooting_direction, 1f, ground_mask);
        if(hit.collider != null && hit.collider.gameObject.tag == "ground")
        {
            Jump();
        }
    }

    void Jump()
    {
        Vector2 up_force = new Vector2(0,30);
        gameObject.GetComponent<Rigidbody2D>().AddForce(up_force);
    }

    Transform FindNearestTower()
    {
        Transform[] towerTransforms = towerContainer.GetComponentsInChildren<Transform>();
        Transform nearest_Transform = transform;
        float min_distance = 0;
        foreach(Transform e in towerTransforms)
        {
            if(e==towerTransforms[0])
            {
                isFindTower = false;
                continue;
            }
            // for the first tower
            if(e==towerTransforms[1])
            {
                isFindTower = true;
                min_distance = CalculateDistanceFromEnemyToTower(e);
                nearest_Transform = e;
                continue;
            }

            // for other towers
            float distance = CalculateDistanceFromEnemyToTower(e);
            if(distance < min_distance)
            {
                min_distance = distance;
                nearest_Transform = e;
            }
        }

        return nearest_Transform;
    }

    // Calculate the distance between tower and enemy
    float CalculateDistanceFromEnemyToTower(Transform towerTransform)
    {
        Vector3 enemyPosition = transform.position;
        Vector3 towerPosition = towerTransform.position;
        float distance = Mathf.Sqrt(Mathf.Pow((towerPosition.x - enemyPosition.x),2) + Mathf.Pow((towerPosition.y - enemyPosition.y),2));
        return distance;
    }

    // Calculate the distance between player and zombie
    float CalculateDistanceToPlayer()
    {
        Vector3 player_position = player.gameObject.transform.position;
        float x1 = transform.position.x;
        float y1 = transform.position.y;
        float x2 = player_position.x;
        float y2 = player_position.y;
        float distance = Mathf.Sqrt((x1-x2)*(x1-x2) + (y1-y2)*(y1-y2));

        return distance;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "tower")
        {
            isTouchTower = true;

            // dash stop
            canUseAbility = false;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(0,0);
        }else if(other.gameObject.tag == "Player")
        {
            isTouchPlayer = true;

            // dash stop
            canUseAbility = false;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(0,0);
        }
    }

    void OnCollisionEnter2D(Collision2D collisionInfo)
    {
        if(collisionInfo.gameObject.tag == "tower")
        {
            isTouchTower = true;

            // dash stop
            canUseAbility = false;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(0,0);
        }else if(collisionInfo.gameObject.tag == "Player")
        {
            isTouchPlayer = true;

            // dash stop
            canUseAbility = false;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(0,0);
        }
    }

    void OnCollisionExit2D(Collision2D collisionInfo)
    {
        if(collisionInfo.gameObject.tag == "tower")
        {
            isTouchTower = false;
        }else if(collisionInfo.gameObject.tag == "Player"){
            isTouchPlayer = false;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.tag == "tower")
        {
            isTouchTower = false;
        }else if(other.gameObject.tag == "Player"){
            isTouchPlayer = false;
        }
    }
}
