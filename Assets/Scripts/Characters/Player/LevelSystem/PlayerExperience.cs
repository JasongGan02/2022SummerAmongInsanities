using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    public float currentExperience = 0;
    public float experienceToNextLevel = 100;

    public void AddExperience(float amount)
    {
        currentExperience += amount;
        CheckForLevelUp();
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
        // Level up logic (increase stats, etc.)
        Debug.Log("Leveled up!");
    }
}