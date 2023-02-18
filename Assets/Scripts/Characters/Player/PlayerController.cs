using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : CharacterController
{

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
    }

    void Start()
    {
        timer = 0f;
        spriteRenderer_component = GetComponent<SpriteRenderer>();
        playermovement_component = GetComponent<Playermovement>();
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
        healthBar = GameObject.Find(Constants.Name.HEALTH_BAR).transform.GetChild(1).GetComponent<Image>();
        if (healthBar != null)
        {
            healthBar.fillAmount = (float) HP / characterStats.HP;
        }
    }

    void FixedUpdate()
    {
        if(GetComponent<Transform>().position.y < -100)
            death();
    }
    public override void death()
    {
        healthBar.fillAmount = 0;
        GameObject.FindObjectOfType<UIViewStateManager>().collaspeAllUI();
        GameObject.FindObjectOfType<UIViewStateManager>().enabled = false;
        Destroy(this.gameObject);
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
