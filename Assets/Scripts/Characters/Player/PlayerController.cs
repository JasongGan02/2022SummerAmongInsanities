using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using System;

[RequireComponent(typeof(AudioEmitter))]
public class PlayerController : CharacterController, IDataPersistence, IAudioable
{
    //Player Run-time only variables
    private float timer;
    private bool isPlayerDead = false; 
    private PlayerMovement playerMovementComponent;
    private PlayerHPUIController hpController;
    private CharacterSpawnManager characterSpawnManager;

     
    private int deathCount = 0;
    private int playerLevel = 0;
    private float playerExperience = 0f;

    Light2D personalLight;
    Light2D globalLight;
    public float intensityThreshold = 0.3f;
    public float checkRadius = 6f;
    public event Action OnWeaponStatsChanged;

    void Start()
    {
        timer = 0f;
        characterSpawnManager = FindObjectOfType<CharacterSpawnManager>();
        playerMovementComponent = GetComponent<PlayerMovement>();
        globalLight = GameObject.Find("BackgroundLight").GetComponent<Light2D>();
        hpController = FindObjectOfType<PlayerHPUIController>();
        hpController.SetupUIElements();
        UpdateHealthUI();
        OnStatsChanged();
    }
    
    public AudioEmitter GetAudioEmitter()
    {
        if (audioEmitter == null)
        {
            Debug.LogError("AudioEmitter is not assigned on " + gameObject.name);
        }
        return audioEmitter;
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
            Die();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (characterObject == null)
            return;
        currentStats.hp = characterObject.maxStats.hp;
        UpdateHealthUI();
    }
    protected override void Update()
    {
        base.Update();
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //TODO: TEMP DISABLE SAVING
            //DataPersistenceManager.instance.SaveGame();
            //SceneManager.LoadSceneAsync("MainMenu");
        }
        
        PlayerSurroundingLight();
    }
    protected override void Die()
    {
        currentStats.hp = 0;
        UpdateHealthUI();
        GameObject.FindObjectOfType<UIViewStateManager>().CollaspeAllUI();
        GameObject.FindObjectOfType<UIViewStateManager>().enabled = false;  
        deathCount++;
        PoolManager.Instance.Return(this.gameObject, characterObject);
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
            WeaponInventory weaponInventory = Array.Find(Resources.FindObjectsOfTypeAll<WeaponInventory>(), w => w.gameObject.activeInHierarchy || !w.gameObject.activeInHierarchy);
            inventory.RemoveAllItemsAndDrops();
            weaponInventory.RemoveAllItemsAndDrops();
            weaponInventory.DestroyAllSpawnedWeapons();

        }
    }
    
    protected override void OnHealthChanged(float hpChange)
    {
        if (hpChange < 0) // Damage taken
        {
            float damageAmount = -hpChange; // Convert to positive value

            if (currentStats.hp <= 0)
            {
                audioEmitter.PlayClipFromCategory("PlayerDeath");
            }
            else
            {
                audioEmitter.PlayClipFromCategory("PlayerInjury");
            }
        }
        else if (hpChange > 0) // Healed
        {
            // Play healing sound or effects if needed
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
            hpController.UpdateHealthUI(currentStats.hp, characterObject.maxStats.hp);
        }
    }

    protected override void OnStatsChanged()
    {
        UpdateHealthUI();
        playerMovementComponent.StatsChange(currentStats.movingSpeed, currentStats.jumpForce, currentStats.totalJumps);
    }
    
    public int GetLevel() { return playerLevel; }
    public float GetEXP() { return playerExperience; }
    public void SetLevel(int newLevel) { playerLevel = newLevel; }
    public void SetEXP(float newEXP) { playerExperience = newEXP; }

    const string HPTEXT_UI_NAME = "HPText";
}
