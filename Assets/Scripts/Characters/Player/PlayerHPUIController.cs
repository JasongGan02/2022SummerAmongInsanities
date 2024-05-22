using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHPUIController : MonoBehaviour
{
    public Image healthBar;
    public Image damagedHealthBar;
    public TextMeshProUGUI healthText;
    public Color damagedColor;
    private float damagedHealthFadeTimer;
    private const float damagedHealthFadeTimerMax = 2f;

    // Initialize references
    public void SetupUIElements()
    {
        healthBar =  GameObject.Find(Constants.Name.HEALTH_BAR).transform.GetChild(2).GetComponent<Image>();
        damagedHealthBar = GameObject.Find(Constants.Name.HEALTH_BAR).transform.GetChild(1).GetComponent<Image>(); 
        healthText = GameObject.Find(HPTEXT_UI_NAME).GetComponent<TextMeshProUGUI>(); // Adjust to actual path
        damagedColor = damagedHealthBar.color;
        damagedColor.a = 0f;
        damagedHealthBar.color = damagedColor;
    }

    // Update the health UI to reflect current health
    public void UpdateHealthUI(float currentHP, float maxHP)
    {
        healthBar.fillAmount = currentHP / maxHP;
        if (healthText != null)
        {
            healthText.text = Mathf.RoundToInt(currentHP).ToString() + "/" + Mathf.RoundToInt(maxHP).ToString();
        }

        if (damagedColor.a > 0)
        {
            damagedHealthFadeTimer -= Time.deltaTime;
            if (damagedHealthFadeTimer < 0)
            {
                damagedColor.a -= 5f * Time.deltaTime;
                if (damagedColor.a < 0) damagedColor.a = 0;
                damagedHealthBar.color = damagedColor;
            }
        }
    }

    // Triggered when damage is taken
    public void ShowDamageEffect()
    {
        damagedHealthBar.fillAmount = healthBar.fillAmount;
        damagedColor.a = 1;
        damagedHealthBar.color = damagedColor;
        damagedHealthFadeTimer = damagedHealthFadeTimerMax;
    }
    
    const string HPTEXT_UI_NAME = "HPText";
}