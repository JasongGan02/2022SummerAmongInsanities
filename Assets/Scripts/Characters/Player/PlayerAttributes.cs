using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttributes : MonoBehaviour
{
    [SerializeField] int MaxPlayerHP;
    [SerializeField] int PlayerStrength;
    [SerializeField] int PlayerSpeed;
    
    float timer;

    bool isPlayerDead;

    int _playerHP;
    int PlayerHP
    {
        get
        {
            return _playerHP;
        }
        set
        {
            _playerHP = value;
            if (healthBar != null)
            {
                healthBar.fillAmount = (float) _playerHP / MaxPlayerHP;
            }
        }
    }

    [SerializeField] float RespwanTimeInterval;

    SpriteRenderer spriteRenderer_component;
    Playermovement playermovement_component;
    CoreArchitecture coreArchitecture;
    Image healthBar;
    
    void Start()
    {
        _playerHP = MaxPlayerHP;
        timer = 0f;
        isPlayerDead = false;
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

        PlayerHP = MaxPlayerHP;
        
        // reset player position
        gameObject.transform.position = coreArchitecture.transform.position;
    }


}
