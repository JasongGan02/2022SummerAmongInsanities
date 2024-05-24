using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEditor.PackageManager;
using Unity.VisualScripting;
using TMPro;

[RequireComponent(typeof(AudioEmitter))]
public class PlayerController : CharacterController, IDataPersistence, IAudioable
{

    private float RespwanTimeInterval;
    //Player Run-time only variables
    private float timer;
    private bool isPlayerDead = false; 
    private Playermovement playermovement_component;
    private PlayerHPUIController hpController;
    private CharacterSpawnManager characterSpawnManager;

     
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
        playermovement_component = GetComponent<Playermovement>();
        globalLight = GameObject.Find("BackgroundLight").GetComponent<Light2D>();
        hpController = FindObjectOfType<PlayerHPUIController>();
        hpController.SetupUIElements();
        UpdateHealthUI();
        EvokeStatsChange();
    }
    
    public AudioEmitter GetAudioEmitter()
    {
        if (_audioEmitter == null)
        {
            Debug.LogError("AudioEmitter is not assigned on " + gameObject.name);
        }
        return _audioEmitter;
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
        
        PlayerSurroundingLight();
    }
    protected override void death()
    {
        _HP = 0;
        UpdateHealthUI();
        GameObject.FindObjectOfType<UIViewStateManager>().collaspeAllUI();
        GameObject.FindObjectOfType<UIViewStateManager>().enabled = false;  
        deathCount++;
        PoolManager.Instance.Return(this.gameObject, characterStats);
        OnObjectReturned(true);
        GameObject WeaponInUse = GameObject.FindWithTag("weapon");
        Destroy(WeaponInUse.GetComponent<Weapon>());
        Destroy(WeaponInUse.GetComponent<Animator>());
        WeaponInUse.AddComponent<DroppedObjectController>();

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


    public override void ApplyHPChange(float dmg)
    {
        if (dmg > 0)
        {
            if (dmg >= _HP)
            {
                _audioEmitter.PlayClipFromCategory("PlayerDeath");
            }
            else
            {
                _audioEmitter.PlayClipFromCategory("PlayerInjury");
            }

        }
        base.ApplyHPChange(dmg);
        hpController.ShowDamageEffect();
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
        if (hpController != null)
        {
            hpController.UpdateHealthUI(_HP, characterStats._HP);
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
