using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class TimeSystemManager : MonoBehaviour
{
    public float dayToRealTimeInSecond;
    public int dayStartHour = 8;
    public int nightStartHour = 20;
    public int redMoonNightInterval = 5;
    public int dawnStartHour = 5; // Hour dawn starts
    public int dawnEndHour = 7; // Hour dawn ends
    public int duskStartHour = 18; // Hour dusk starts
    public int duskEndHour = 20; // Hour dusk ends
    public bool isDebugDayTime = false;

    public Action onDayStarted { get; set; }
    public Action onDuskStarted { get; }
    public Action<bool> onNightStarted { get; set; }

    public Action<int> onHourUpdated { get; set; }
    public Action<int> onDayUpdated { get; set; }

    private TMP_Text timeText; //x��xʱ
    private float currentMinute = 0;
    private int currentHour = 0;
    private int currentDay = 0;
    
    //LightManager variables
    private int lastSunlightLevelUpdate = -1;
    private float maxSunlightLevel = 15; 
    private float minSunlightLevel = 4;

    private float gameMinuteInRealSec;
    private audioManager am;
    // Start is called before the first frame update
    void Awake()
    {
        timeText = GameObject.Find(Constants.Name.TIME_TEXT).GetComponent<TMP_Text>();
        gameMinuteInRealSec = 24f * 60f / dayToRealTimeInSecond;
        am = GameObject.FindGameObjectWithTag("audio").GetComponent<audioManager>();
        if (isDebugDayTime) SetToDaytime();
        else InitializeTimeBasedOnCurrentHour();
    }
    
    private void SetToDaytime()
    {
        currentHour = dayStartHour + 1;
        onDayStarted?.Invoke();
        am.playBGM(am.DayTime);
    }
    
    private void InitializeTimeBasedOnCurrentHour()
    {
        if (currentHour >= dayStartHour && currentHour < nightStartHour)
        {
            am.playBGM(am.DayTime);
            onDayStarted?.Invoke();
        }
        else
        {
            am.playBGM(am.NightTime);
            onNightStarted?.Invoke(currentDay != 0 && currentDay % redMoonNightInterval == 0);
        }
    }

    public int GetDayTimeLengthInHour()
    {
        return nightStartHour - dayStartHour;
    }
    
    private int GetNightTimePassedForToday()
    {
        if (currentHour >= nightStartHour)
        {
            return currentHour - nightStartHour;
        }
        else
        {
            return 24 - nightStartHour + currentHour;
        }
    }

    public bool IsInDaytime()
    {
        
        return currentHour >= dayStartHour && currentHour < nightStartHour;
        
    }

    public float GetHowMuchPercentageOfNightTimeHasPassed()
    {
        if (IsInDaytime()) return 0;
        return (float) GetNightTimePassedForToday() / (24 - GetDayTimeLengthInHour());
    }
    
    private void IncrementTime(float gameMinutesToAdvance)
    {
        currentMinute += gameMinutesToAdvance;

        // Check for minute overflow and increment hours accordingly.
        if (currentMinute >= 60)
        {
            currentHour += (int)(currentMinute / 60);
            currentMinute %= 60;

            // Trigger any hourly updates here
            onHourUpdated?.Invoke(currentHour);
        }

        // Check for hour overflow and increment days accordingly.
        if (currentHour >= 24)
        {
            currentDay++;
            currentHour %= 24;

            // Trigger any daily updates here
            onDayUpdated?.Invoke(currentDay);
        }
    }
    
    void FixedUpdate()
    {
        float gameTimeAdvancePerFixedUpdate = gameMinuteInRealSec * Time.fixedDeltaTime;

        // Now advance the game time by this amount in each FixedUpdate call.
        AdvanceGameTime(gameTimeAdvancePerFixedUpdate);
    }

    void AdvanceGameTime(float gameMinutesToAdvance)
    {
        if (!isDebugDayTime) IncrementTime(gameMinutesToAdvance);
        // Update the UI and check for day/night transition as before.
        UpdateTimeUI();
        CheckForDayNightTransition();
        CalculateAndUpdateSunlight();
    }

    private bool dayStarted = false;
    private bool nightStarted = false;
    private void CheckForDayNightTransition()
    {
        if (currentHour == dayStartHour && !dayStarted)
        {
            onDayStarted?.Invoke();
            dayStarted = true; // Mark the day start transition as handled
            nightStarted = false;
            am.playBGM(am.DayTime);
        }
        else if (currentHour == nightStartHour && !nightStarted)
        {
            onNightStarted?.Invoke(currentDay != 0 && currentDay % redMoonNightInterval == 0);
            nightStarted = true; // Mark the night start transition as handled
            dayStarted = false;
            am.playBGM(am.NightTime);
        }
    }

    private void UpdateTimeUI()
    {
        timeText.text = $"{currentDay}天{currentHour}时";
    }
    public float GetCurrentTime() => currentHour + currentMinute / 60f;
    public void CalculateAndUpdateSunlight()
    {
        float currentHour = GetCurrentTime();
        int sunlightLevel = Mathf.RoundToInt(CalculateSunlightLevel(currentHour));

        if (sunlightLevel != lastSunlightLevelUpdate)
        {
            LightGenerator.Instance.UpdateSunlightBrightness(sunlightLevel);
            lastSunlightLevelUpdate = sunlightLevel;
        }
    }

    private float CalculateSunlightLevel(float currentHour)
    {
        float sunlightLevel;
        if (currentHour >= dawnEndHour && currentHour < duskStartHour)
        {
            sunlightLevel = maxSunlightLevel;
        }
        else if (currentHour >= duskEndHour || currentHour < dawnStartHour)
        {
            sunlightLevel = minSunlightLevel;
        }
        else
        {
            if (currentHour >= dawnStartHour && currentHour < dawnEndHour)
            {
                float progress = (currentHour - dawnStartHour) / (dawnEndHour - dawnStartHour);
                sunlightLevel = Mathf.Lerp(minSunlightLevel, maxSunlightLevel, progress);
            }
            else if (currentHour >= duskStartHour && currentHour < duskEndHour)
            {
                float progress = (currentHour - duskStartHour) / (duskEndHour - duskStartHour);
                sunlightLevel = Mathf.Lerp(maxSunlightLevel, minSunlightLevel, progress);
            }
            else
            {
                sunlightLevel = minSunlightLevel;
            }
        }

        return sunlightLevel;
    }
    
    

}
