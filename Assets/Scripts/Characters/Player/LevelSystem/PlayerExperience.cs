using System;
using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    public int Level { get; private set; } = 1;
    public float currentExperience;
    public float experienceToNextLevel = 10;
    public float currentAsh;

    private PlayerExperienceUI playerExperienceUI;

    private void Start()
    {
        playerExperienceUI = FindObjectOfType<PlayerExperienceUI>();
        if (playerExperienceUI == null)
        {
            Debug.LogError("playerExperience Ui is not in the scene");
        }
        playerExperienceUI.UpdateExperienceUI(currentExperience, experienceToNextLevel);
        playerExperienceUI.UpdateAshUI(currentAsh);
    }


    public void AddExperience(float amount)
    {
        currentExperience += amount;
        CheckForLevelUp();
        playerExperienceUI.UpdateExperienceUI(currentExperience, experienceToNextLevel);
    }

    public void AddAsh(float amount)
    {
        currentAsh += amount;
        playerExperienceUI.UpdateAshUI(currentAsh);
    }

    public bool SpendAsh(float amount)
    {
        if (amount >= currentAsh)
        {
            currentAsh -= amount;
            return true;
        }

        return false;
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
        Debug.Log("Leveled up!");
        
        // Fire the LevelUp event via GameEvents
        if (GameEvents.current != null)
        {
            GameEvents.current.PlayerLevelUp();
        }
        else
        {
            Debug.LogError("GameEvents instance not found in the scene.");
        }
    }
}