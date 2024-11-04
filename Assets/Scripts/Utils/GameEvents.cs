using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents current;

    private void Awake()
    {
        if (current != null && current != this)
        {
            // If a duplicate exists, destroy it    
            Destroy(gameObject);
        }
        else
        {
            current = this;
            DontDestroyOnLoad(gameObject); // Make this object persistent
        }
    }

    public event Action<int> OnEnemyLevelIncreased;

    public void IncreaseEnemyLevel(int enemyLevel)
    {
        OnEnemyLevelIncreased?.Invoke(enemyLevel);
    }
    
    public event Action OnDayStarted;
    public event Action OnDuskStarted;
    public event Action<bool> OnNightStarted;
    public event Action<int> OnHourUpdated;
    public event Action<int> OnDayUpdated;
    
    public void DayStarted()
    {
        OnDayStarted?.Invoke();
    }

    public void DuskStarted()
    {
        OnDuskStarted?.Invoke();
    }

    public void NightStarted(bool isRedMoonNight)
    {
        OnNightStarted?.Invoke(isRedMoonNight);
    }

    public void HourUpdated(int hour)
    {
        OnHourUpdated?.Invoke(hour);
    }

    public void DayUpdated(int day)
    {
        OnDayUpdated?.Invoke(day);
    }
    
    public event Action OnPlayerLevelUp;

    public void PlayerLevelUp()
    {
        OnPlayerLevelUp?.Invoke();
    }
    
    public event Action OnOpenSacrificeStore;

    public void OpenSacrificeStore()
    {
        OnOpenSacrificeStore?.Invoke();
    }
}
