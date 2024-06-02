using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class PlayerHPUIController : MonoBehaviour
{
    public Image fillImage;
    //public Image damagedHealthBar;
    public Text hpLightText;
    public Text hpDarkText;

    public Image redMoonDeco;

    public Image TimeSystemUI_Base;

    public Sprite[] timeUIBaseSprites; //0 = normal 1 = blood moon
    //public Color damagedColor;
    //private float damagedHealthFadeTimer;
    //private const float damagedHealthFadeTimerMax = 2f;
    
    void Start()
    {
        GameEvents.current.OnDayStarted += ChangeToSun;
        GameEvents.current.OnNightStarted += ChangeToMoon;
    }
    // Initialize references
    public void SetupUIElements()
    {
        //damagedColor = damagedHealthBar.color;
        //damagedColor.a = 0f;
        //damagedHealthBar.color = damagedColor;
    }

    // Update the health UI to reflect current health
    public void UpdateHealthUI(float currentHP, float maxHP)
    {
        float fillAmount = currentHP / maxHP;
        fillImage.fillAmount = fillAmount;
        if (hpLightText != null)
        {
            hpLightText.text = Mathf.RoundToInt(currentHP).ToString() + "/" + Mathf.RoundToInt(maxHP).ToString();
            hpDarkText.text = Mathf.RoundToInt(currentHP).ToString() + "/" + Mathf.RoundToInt(maxHP).ToString();
        }

        // if (damagedColor.a > 0)
        // {
        //     damagedHealthFadeTimer -= Time.deltaTime;
        //     if (damagedHealthFadeTimer < 0)
        //     {
        //         damagedColor.a -= 5f * Time.deltaTime;
        //         if (damagedColor.a < 0) damagedColor.a = 0;
        //         damagedHealthBar.color = damagedColor;
        //     }
        // }
    }

    // Triggered when damage is taken
    public void ShowDamageEffect()
    {
        // damagedHealthBar.fillAmount = fillImage.fillAmount;
        // damagedColor.a = 1;
        // damagedHealthBar.color = damagedColor;
        // damagedHealthFadeTimer = damagedHealthFadeTimerMax;
    }
    
    private void ChangeToSun()
    {
        TimeSystemUI_Base.sprite = timeUIBaseSprites[0];
        redMoonDeco.gameObject.SetActive(false);
    }

    private void ChangeToMoon(bool isRedMoon)
    {
        if (isRedMoon)
        {
            TimeSystemUI_Base.sprite = timeUIBaseSprites[1];
            redMoonDeco.gameObject.SetActive(true);
        }
        else
        {
            TimeSystemUI_Base.sprite = timeUIBaseSprites[0];
            redMoonDeco.gameObject.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        GameEvents.current.OnDayStarted -= ChangeToSun;
        GameEvents.current.OnNightStarted -= ChangeToMoon;
    }
    
    const string HPTEXT_UI_NAME = "HPText";
}