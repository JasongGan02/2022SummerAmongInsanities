using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : CharacterController
{

    protected float RespwanTimeInterval;


    //Player Run-time only variables
    float timer;
    bool isPlayerDead = false; 
    SpriteRenderer spriteRenderer_component;
    Playermovement playermovement_component;
    CoreArchitecture coreArchitecture;
    Image healthBar;
    
    public virtual void Initialize(CharacterObject character, float HP, float AtkDamage, float AtkInterval, float MovingSpeed, float RespwanTimeInterval)
    {
        base.Initialize(character, HP, AtkDamage, AtkInterval, MovingSpeed);
        this.RespwanTimeInterval = RespwanTimeInterval;
    }

    void Start()
    {
        timer = 0f;
        spriteRenderer_component = GetComponent<SpriteRenderer>();
        playermovement_component = GetComponent<Playermovement>();
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
        healthBar = GameObject.Find(Constants.Name.HEALTH_BAR).transform.GetChild(1).GetComponent<Image>();
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
    void PlayerRespawn()
    {
        isPlayerDead = false;
        spriteRenderer_component.enabled = true;
        playermovement_component.enabled = true;

        HP = characterStats.HP;
        
        // reset player position
        gameObject.transform.position = coreArchitecture.transform.position;
    }
    public override void death()
    {
        isPlayerDead = true;
        spriteRenderer_component.enabled = false;
        playermovement_component.enabled = false;
    }

    public override void takenDamage(float dmg)
    {
        base.takenDamage(dmg);
        if (healthBar != null)
        {
            healthBar.fillAmount = (float) HP / characterStats.HP;
        }
    }

}
