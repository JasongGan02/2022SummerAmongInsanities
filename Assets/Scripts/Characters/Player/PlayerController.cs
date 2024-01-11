using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEditor.PackageManager;
using Unity.VisualScripting;
using TMPro;

public class PlayerController : CharacterController, IDataPersistence
{

    private float RespwanTimeInterval;
    //Player Run-time only variables
    float timer;
    bool isPlayerDead = false; 
    SpriteRenderer spriteRenderer_component;
    Playermovement playermovement_component;
    CoreArchitecture coreArchitecture;
    private CharacterSpawnManager characterSpawnManager;


    //UI Elements
    Image healthBar;
    Image damagedHealthBar;
    TextMeshProUGUI healthText;
    Color damagedColor;
    float damagedHealthFadeTimer;
    float damaged_health_fade_timer_max = 2f;
     
    private int deathCount = 0;
    private int playerLevel = 0;
    private float playerExperience = 0f;

    Light2D personalLight;
    Light2D globalLight;
    public float intensityThreshold = 0.3f;
    public float checkRadius = 6f;

    void Start()
    {
        timer = 0f;
        characterSpawnManager = FindObjectOfType<CharacterSpawnManager>();
        spriteRenderer_component = GetComponent<SpriteRenderer>();
        playermovement_component = GetComponent<Playermovement>();
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
        healthBar = GameObject.Find(Constants.Name.HEALTH_BAR).transform.GetChild(2).GetComponent<Image>();
        damagedHealthBar = GameObject.Find(Constants.Name.HEALTH_BAR).transform.GetChild(1).GetComponent<Image>();
        damagedColor = damagedHealthBar.color;
        damagedColor.a = 0f;
        damagedHealthBar.color = damagedColor;
        globalLight = GameObject.Find("BackgroundLight").GetComponent<Light2D>();
        healthText = GameObject.Find(HPTEXT_UI_NAME).GetComponent<TextMeshProUGUI>(); // Replace with your actual object name
        UpdateHealthUI();
        EvokeStatsChange();
        am = GameObject.FindGameObjectWithTag("audio").GetComponent<audioManager>();
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

    void OnEnable()
    {
        if (characterStats == null)
            return;
        _HP = characterStats._HP;
        UpdateHealthUI();
    }
    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DataPersistenceManager.instance.SaveGame();
            SceneManager.LoadSceneAsync("MainMenu");
        }
        
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

        PlayerSurroundingLight();
    }
    public override void death()
    {
            
        
        _HP = 0;
        UpdateHealthUI();
        GameObject.FindObjectOfType<UIViewStateManager>().collaspeAllUI();
        GameObject.FindObjectOfType<UIViewStateManager>().enabled = false;  
        deathCount++;
        PoolManager.Instance.Return(this.gameObject, characterStats);
        OnObjectReturned(true);
         GameObject WeaponInUse = GameObject.FindWithTag("weapon");
        if (WeaponInUse != null)
        {
            Destroy(WeaponInUse);
        }
        GameObject TowerInUse = GameObject.FindWithTag("tower");
        if (TowerInUse != null)
        {
            Destroy(TowerInUse);
        }
        if (characterSpawnManager != null)
        {
            characterSpawnManager.StartRespawnCoroutine();
        }
    }
 
    public override void Reinitialize()
    {
        base.Reinitialize(); //Reset Stats
        GameObject.FindObjectOfType<UIViewStateManager>().enabled = true;
        // Update the UI
        UpdateHealthUI();
    }
    protected override void OnObjectReturned(bool playerDropItemsOnDeath)
    {
        if (playerDropItemsOnDeath)
        {
            Inventory inventory = FindObjectOfType<Inventory>();
            inventory.RemoveAllItemsAndDrops();
        }
    }


    public override void takenDamage(float dmg)
    {
        base.takenDamage(dmg);
        
        if (dmg > 0)
        {
            if(dmg > _HP)
            {
                am.playAudio(am.death);
            }
            else
            {
                am.playAudio(am.injured[UnityEngine.Random.Range(0, am.injured.Length)]);
            }
            
        }
        if (damagedColor.a <= 0)
        {   // Damaged Bar is invisible
            damagedHealthBar.fillAmount = healthBar.fillAmount;
        }
        damagedColor.a = 1;
        damagedHealthBar.color = damagedColor;
        damagedHealthFadeTimer = damaged_health_fade_timer_max;

        UpdateHealthUI();
    }

    public void Heal(float amount)
    {
        _HP += amount;
        if (_HP > characterStats._HP)
        {
            _HP = characterStats._HP;
        }
        UpdateHealthUI();
    }

    public void PlayerSurroundingLight()
    {
        personalLight = GetComponent<Light2D>();

        if (personalLight != null && globalLight.intensity < 0.5f)
        {
            personalLight.intensity = 0.6f - globalLight.intensity;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, checkRadius);

            foreach (var collider in colliders)
            {
                Light2D otherLight = collider.GetComponent<Light2D>();
                if (otherLight != null && otherLight != personalLight)
                {
                    if (otherLight.intensity > intensityThreshold)
                    {
                        personalLight.intensity = 0f; // Turn off the personal light
                        return;
                    }
                }
            }

            personalLight.intensity = 0.6f - globalLight.intensity; // Turn on the personal light
        }
        else
        {
            personalLight.intensity = 0f;
        }
    }
    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)_HP / characterStats._HP;
        }

        if (healthText != null)
        {
            // Round HP to the nearest integer
            int roundedHP = Mathf.RoundToInt(_HP);
            int maxHP = Mathf.RoundToInt(characterStats._HP); // Assuming max HP should also be an integer

            healthText.text = roundedHP.ToString() + "/" + maxHP.ToString();
        }
    }

    protected override void EvokeStatsChange()
    {
        playermovement_component.StatsChange(_movingSpeed, _jumpForce, _totalJumps);
    }
    
    public float GetPersonalLight() { return personalLight.intensity; }

    public int GetLevel() { return playerLevel; }
    public float GetEXP() { return playerExperience; }
    public void SetLevel(int newLevel) { playerLevel = newLevel; }
    public void SetEXP(float newEXP) { playerExperience = newEXP; }

    const string HPTEXT_UI_NAME = "HPText";
}
