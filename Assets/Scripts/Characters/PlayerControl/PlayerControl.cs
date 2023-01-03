using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : CharacterPanel
{
    [SerializeField] 
    public float RespwanTimeInterval = 5f;
    
    float timer =0f;
    bool isPlayerDead = false;
    SpriteRenderer spriteRenderer_component;
    EdgeCollider2D edgeCollider2D_component;
    Playermovement playermovement_component;
    CoreArchitecture coreArchitecture;
    Rigidbody2D rigidbody2D_component;
    PlayerObject player;
    
    void Awake()
    {
        maxHealth = 10f;
        player = new PlayerObject(10f);

    }

    void Start()
    {
        isPlayerDead = false;
        spriteRenderer_component = GetComponent<SpriteRenderer>();
        playermovement_component = GetComponent<Playermovement>();
        edgeCollider2D_component = GetComponent<EdgeCollider2D>();
        rigidbody2D_component = GetComponent<Rigidbody2D>();
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
    }

    void Update()
    {
        if(isPlayerDead)
        {
            
            timer += Time.deltaTime;
            if(timer >= RespwanTimeInterval)
            {
                PlayerRespawn();
                timer = 0f;
            }
        }
    }

    protected override void Death()
    {
        
        isPlayerDead = true;       
        spriteRenderer_component.enabled = false;
        playermovement_component.enabled = false;
        edgeCollider2D_component.isTrigger = true;
        rigidbody2D_component.simulated = false;
    }

    void PlayerRespawn()
    {
        isPlayerDead = false;
        spriteRenderer_component.enabled = true;
        playermovement_component.enabled = true;
        edgeCollider2D_component.isTrigger = false;
        rigidbody2D_component.simulated = true;
        curHealth = maxHealth;
        // reset player position
        gameObject.transform.position = coreArchitecture.transform.position;
    }
}
