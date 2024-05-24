using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class PlayerHPUIController : MonoBehaviour
{
    public Image fillImage;
    public Image headImage;
    //public Image damagedHealthBar;
    public TextMeshProUGUI healthText;
    //public Color damagedColor;
    //private float damagedHealthFadeTimer;
    //private const float damagedHealthFadeTimerMax = 2f;

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
        if (healthText != null)
        {
            healthText.text = Mathf.RoundToInt(currentHP).ToString() + "/" + Mathf.RoundToInt(maxHP).ToString();
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
        
        if (headImage != null)
        {
            float fillWidth = fillImage.rectTransform.rect.width * fillImage.rectTransform.localScale.x;  
            float headWidth = headImage.rectTransform.rect.width * headImage.rectTransform.localScale.x;
            
            // New head position calculation
            Vector3 headPosition = headImage.rectTransform.localPosition;
            headPosition.x = fillImage.rectTransform.rect.x + (fillWidth * fillAmount);
            headImage.rectTransform.localPosition = headPosition;
        }
    }

    // Triggered when damage is taken
    public void ShowDamageEffect()
    {
        // damagedHealthBar.fillAmount = fillImage.fillAmount;
        // damagedColor.a = 1;
        // damagedHealthBar.color = damagedColor;
        // damagedHealthFadeTimer = damagedHealthFadeTimerMax;
    }
    
    const string HPTEXT_UI_NAME = "HPText";
}