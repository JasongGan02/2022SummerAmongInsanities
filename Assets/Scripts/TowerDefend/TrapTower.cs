using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Trap tower can shoot horizontally and vertically
// The bullet of trap tower have no gravity
// 
public class TrapTower : MonoBehaviour
{
    LayerMask enemyLayer;
    bool isUpEnemy;
    bool isDownEnemy;
    bool isRightEnemy;
    bool isLeftEnemy;
    enum Direction{up, right, down, left};
    [SerializeField] float SenseDistance = 100.0f;
    [SerializeField] GameObject bullet;
    [SerializeField] float bullet_speed;    // bullet flying speed
    [SerializeField] float AtkSpeed;        // Atk Speed affects the Atk Timer counting. When AtkSpeed = 1, the time elapsed speed = real time elapsed speed
    [SerializeField] float AtkIntervalTime; // During AtkIntervalTime, the tower will automatically attack once
    float AtkTimer;        // Timer
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

    void FixedUpdate()
    {
        SenseEnemy();
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
            print("input wrong direction");
            break;
        }

        // 朝向量方向发射子弹
        GameObject bullet_instance = Instantiate(bullet, transform.position, Quaternion.identity);
        // Give bullet speed
        bullet_instance.GetComponent<Rigidbody2D>().velocity = shootingDirection * bullet_speed;
    }

    // Use raycast horizontally and verticallly to sense enemy
    // There will be 4 directions, it will sense seperatelly
    void SenseEnemy()
    {
        // Shoot 4 lines into 4 directions
        RaycastHit2D hit_right = Physics2D.Raycast(transform.position, Vector2.right, SenseDistance, enemyLayer);
        if(hit_right)
        {
            isRightEnemy = true;
        }else{
            isRightEnemy = false;
        }

        RaycastHit2D hit_left = Physics2D.Raycast(transform.position, Vector2.left, SenseDistance, enemyLayer);
        if(hit_left)
        {
            isLeftEnemy = true;
        }else{
            isLeftEnemy = false;
        }

        RaycastHit2D hit_down = Physics2D.Raycast(transform.position, Vector2.down, SenseDistance, enemyLayer);
        if(hit_down)
        {
            isDownEnemy = true;
        }else{
            isDownEnemy = false;
        }

        RaycastHit2D hit_up = Physics2D.Raycast(transform.position, Vector2.up, SenseDistance, enemyLayer);
        if(hit_up)
        {
            isUpEnemy = true;
        }else{
            isUpEnemy = false;
        }
    }
}
