using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerExperienceUI : MonoBehaviour
{
    public Image experienceFillImage; // Bar to represent experience
    public Text exLightText;
    public Text exDarkText;

    // Update the experience UI to reflect the current experience
    public void UpdateExperienceUI(float currentXP, float maxXP)
    {
        float fillAmount = currentXP / maxXP;
        experienceFillImage.fillAmount = fillAmount;

        if (exLightText != null)
        {
            exLightText.text = Mathf.RoundToInt(currentXP).ToString() + "/" + Mathf.RoundToInt(maxXP).ToString();
            exDarkText.text = Mathf.RoundToInt(currentXP).ToString() + "/" + Mathf.RoundToInt(maxXP).ToString();
        }
    }
}