using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Trap tower can shoot horizontally and vertically
// The bullet of trap tower have no gravity
// 
public class TrapTowerController : TowerController
{
    LayerMask enemyLayer;
    bool isUpEnemy;
    bool isDownEnemy;
    bool isRightEnemy;
    bool isLeftEnemy;
    enum Direction{up, right, down, left};
    [SerializeField] float SenseDistance = 100.0f;
    [SerializeField] float AtkSpeed;        // Atk Speed affects the Atk Timer counting. When AtkSpeed = 1, the time elapsed speed = real time elapsed speed
    [SerializeField] float AtkIntervalTime; // During AtkIntervalTime, the tower will automatically attack once
    [SerializeField] Direction atkDirection;    //  left Z = 0 down Z = 90  right Z = 180   up Z = 270

    // Start is called before the first frame update
    void Start()
    {
        isUpEnemy = false;
        isRightEnemy = false;
        isLeftEnemy = false;
        isDownEnemy = false;
        enemyLayer = LayerMask.GetMask("enemy");
        AtkTimer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        SetDirection();
        if(isDownEnemy||isRightEnemy||isLeftEnemy||isUpEnemy)
        {
            AtkTimer += Time.deltaTime * AtkSpeed;
            if(AtkTimer >= AtkIntervalTime)
            {
                CheckAndShoot();
                AtkTimer = 0f;
            }
        }
        
    }
    
    // Sense enemy in fixed update
    void FixedUpdate()
    {
        SenseEnemy();
    }

    void SetDirection()
    {
        switch(atkDirection)
        {
            case Direction.left:
            transform.rotation = Quaternion.Euler(0,0,0);
            break;
            case Direction.down:
            transform.rotation = Quaternion.Euler(0,0,90);
            break;
            case Direction.right:
            transform.rotation = Quaternion.Euler(0,0,180);
            break;
            case Direction.up:
            transform.rotation = Quaternion.Euler(0,0,270);
            break;
            default:
            Debug.LogWarning("atkDirection is not setted");
            break;
        }
    }

    

    void CheckAndShoot()
    {
        if(isDownEnemy)
        {
            ShootOnce(Direction.down);
        }
        if(isUpEnemy)
        {
            ShootOnce(Direction.up);
        }
        if(isRightEnemy)
        {
            ShootOnce(Direction.right);
        }
        if(isLeftEnemy)
        {
            ShootOnce(Direction.left);
        }
    }

    // Shoot one bullet into direction
    void ShootOnce(Direction direction)
    {
        Vector2 shootingDirection = new Vector2(0,0);
        switch(direction)
        {
            case Direction.up:
            shootingDirection = Vector2.up;
            break;
            case Direction.right:
            shootingDirection = Vector2.right;
            break;
            case Direction.down:
            shootingDirection = Vector2.down;
            break;
            case Direction.left:
            shootingDirection = Vector2.left;
            break;
            default:
            Debug.LogWarning("direction is not setted");
            break;
        }
        GameObject bullet_instance = Instantiate(bullet, transform.position, Quaternion.identity);
        // Give bullet speed
        bullet_instance.GetComponent<Rigidbody2D>().velocity = shootingDirection * bullet_speed;
    }

    // Use raycast horizontally and verticallly to sense enemy
    // The raycast will shoot into atkDirection
    void SenseEnemy()
    {
        isRightEnemy = false;
        isLeftEnemy = false;
        isUpEnemy = false;
        isDownEnemy = false;
        // Shoot 4 lines into 4 directions
        switch(atkDirection)
        {
            case Direction.left:
            RaycastHit2D hit_left = Physics2D.Raycast(transform.position, Vector2.left, SenseDistance, enemyLayer);
            if(hit_left)
            {
                isLeftEnemy = true;
            }else{
                isLeftEnemy = false;
            }
            break;
            case Direction.down:
            RaycastHit2D hit_down = Physics2D.Raycast(transform.position, Vector2.down, SenseDistance, enemyLayer);
            if(hit_down)
            {
                isDownEnemy = true;
            }else{
                isDownEnemy = false;
            }
            break;
            case Direction.right:
            RaycastHit2D hit_right = Physics2D.Raycast(transform.position, Vector2.right, SenseDistance, enemyLayer);
            if(hit_right)
            {
                isRightEnemy = true;
            }else{
                isRightEnemy = false;
            }
            break;
            case Direction.up:
            RaycastHit2D hit_up = Physics2D.Raycast(transform.position, Vector2.up, SenseDistance, enemyLayer);
            if(hit_up)
            {
                isUpEnemy = true;
            }else{
                isUpEnemy = false;
            }
            break;
            default:
            Debug.LogWarning("atkDirection is not setted");
            break;
        }
    }
}
