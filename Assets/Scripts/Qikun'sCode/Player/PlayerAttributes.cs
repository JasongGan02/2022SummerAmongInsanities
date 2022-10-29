using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttributes : MonoBehaviour
{
    [SerializeField] int PlayerHP;
    [SerializeField] int PlayerStrength;
    [SerializeField] int PlayerSpeed;



    float timer;

    bool isPlayerDead;

    [SerializeField] float RespwanTimeInterval;

    SpriteRenderer spriteRenderer_component;
    Playermovement playermovement_component;
    CoreArchitecture coreArchitecture;
    
    void Start()
    {
        timer = 0f;
        isPlayerDead = false;
        spriteRenderer_component = GetComponent<SpriteRenderer>();
        playermovement_component = GetComponent<Playermovement>();
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

    public void DecreaseHealth(int damage)
    {
        PlayerHP -= damage;

        if(PlayerHP <= 0)
        {
            PlayerDead();
        }
    }

    void PlayerDead()
    {
        
        isPlayerDead = true;
        spriteRenderer_component.enabled = false;
        playermovement_component.enabled = false;
    }

    void PlayerRespawn()
    {
        isPlayerDead = false;
        spriteRenderer_component.enabled = true;
        playermovement_component.enabled = true;

        PlayerHP = 10;
        

        // reset player position
        gameObject.transform.position = coreArchitecture.transform.position;
    }

}
