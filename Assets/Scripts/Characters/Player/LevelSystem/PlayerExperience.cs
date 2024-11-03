using System;
using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    public int Level { get; private set; } = 1;
    public float currentExperience = 0;
    public float experienceToNextLevel = 10;

    private PlayerExperienceUI playerExperienceUI;

    private void Start()
    {
        playerExperienceUI = FindObjectOfType<PlayerExperienceUI>();
        if (playerExperienceUI == null)
        {
            Debug.LogError("playerExperience Ui is not in the scene");
        }
        playerExperienceUI.UpdateExperienceUI(currentExperience, experienceToNextLevel);
    }


    public void AddExperience(float amount)
    {
        currentExperience += amount;
        CheckForLevelUp();
        playerExperienceUI.UpdateExperienceUI(currentExperience, experienceToNextLevel);
    }

    private void CheckForLevelUp()
    {
        if (currentExperience >= experienceToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentExperience -= experienceToNextLevel;
        experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.1f); // Example scaling
        Level++;
        // Level up logic (increase stats, etc.)
        Debug.Log("Leveled up!");
    }
}