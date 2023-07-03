using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : CharacterController, IDataPersistence
{

    private float RespwanTimeInterval;
    //Player Run-time only variables
    float timer;
    bool isPlayerDead = false; 
    SpriteRenderer spriteRenderer_component;
    Playermovement playermovement_component;
    CoreArchitecture coreArchitecture;
    Image healthBar;
    Image damagedHealthBar;
    Color damagedColor;
    float damagedHealthFadeTimer;
    float damaged_health_fade_timer_max = 2f; 

    private int deathCount = 0;
    private int playerLevel = 0;
    private float playerExperience = 0f;

    void Start()
    {
        timer = 0f;
        spriteRenderer_component = GetComponent<SpriteRenderer>();
        playermovement_component = GetComponent<Playermovement>();
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
        healthBar = GameObject.Find(Constants.Name.HEALTH_BAR).transform.GetChild(2).GetComponent<Image>();
        damagedHealthBar = GameObject.Find(Constants.Name.HEALTH_BAR).transform.GetChild(1).GetComponent<Image>();
        damagedColor = damagedHealthBar.color;
        damagedColor.a = 0f;
        damagedHealthBar.color = damagedColor;
        if (healthBar != null)
        {
            healthBar.fillAmount = (float) HP / characterStats.HP;
        }
    }

    public void LoadData(GameData data)
    {
        //this.deathCount = data.deathCount;
    }

    public void SaveData(GameData data)
    {
        //data.deathCount = this.deathCount;
    }

    void FixedUpdate()
    {
        if(GetComponent<Transform>().position.y < -100)
            death();
    }
    protected override void Update()
    {
        base.Update();
        if (damagedColor.a > 0)
        {
            damagedHealthFadeTimer -= Time.deltaTime;
            if (damagedHealthFadeTimer < 0)
            {
                float fadeAmount = 5f;
                damagedColor.a -= fadeAmount * Time.deltaTime;
                damagedHealthBar.color = damagedColor;
            }
        }
    }
    public override void death()
    {
        healthBar.fillAmount = 0;
        GameObject.FindObjectOfType<UIViewStateManager>().collaspeAllUI();
        GameObject.FindObjectOfType<UIViewStateManager>().enabled = false;
        deathCount++;
        Destroy(this.gameObject);

    }

    public override void takenDamage(float dmg)
    {
        HP -= dmg;
        if (HP <= 0)
        {
            death();
        }
        StartCoroutine(FlashRed());

        if (damagedColor.a <= 0)
        {   // Damaged Bar is invisible
            damagedHealthBar.fillAmount = healthBar.fillAmount;
        }
        damagedColor.a = 1;
        damagedHealthBar.color = damagedColor;
        damagedHealthFadeTimer = damaged_health_fade_timer_max;

        if (healthBar != null)
        {
            healthBar.fillAmount = (float) HP / characterStats.HP;
        }
    }

    public void Heal(float amount)
    {
        HP += amount;
        if (HP > characterStats.HP)
        {
            HP = characterStats.HP;
        }
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)HP / characterStats.HP;
        }
    }

    public int GetLevel() { return playerLevel; }
    public float GetEXP() { return playerExperience; }
    public void SetLevel(int newLevel) { playerLevel = newLevel; }
    public void SetEXP(float newEXP) { playerExperience = newEXP; }
}
