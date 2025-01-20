using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
    public bool isDay;
    public bool isDebugDayTime = false;
    
    
    private Text timeText; //hours and minutes
    private Text calendarText; //days
    private float currentMinute = 0;
    private int currentHour = 0;
    private int currentDay = 0;
    
    //LightManager variables
    private int lastSunlightLevelUpdate = -1;
    private float maxSunlightLevel = 15; 
    private float minSunlightLevel = 4;

    private float gameMinuteInRealSec;
    private AudioEmitter _audioEmitter;
    // Start is called before the first frame update
    void Awake()
    {
       //timeText = GameObject.Find(Constants.Name.TIME_TEXT).GetComponent<Text>();
        //calendarText = GameObject.Find(Constants.Name.CALENDAR_TEXT).GetComponent<Text>();
        gameMinuteInRealSec = 24f * 60f / dayToRealTimeInSecond;
        _audioEmitter = GetComponent<AudioEmitter>();
    }


    private void Start()
    {
        if (isDebugDayTime) SetToDaytime();
        else InitializeTimeBasedOnCurrentHour();
    }

    private void SetToDaytime()
    {
        currentHour = dayStartHour + 1;
        GameEvents.current.DayStarted();
        isDay = true;
    }
    
    private void InitializeTimeBasedOnCurrentHour()
    {
        if (currentHour >= dayStartHour && currentHour < nightStartHour)
        {
            _audioEmitter.PlayClipFromCategory("DayTime", false);
            _audioEmitter.IsPlaying("DayTime");
            GameEvents.current.DayStarted();
            isDay = true;
            GameEvents.current.HourUpdated(currentHour);
        }
        else
        {
            GameEvents.current.NightStarted(currentDay != 0 && currentDay % redMoonNightInterval == 0);
            GameEvents.current.HourUpdated(currentHour);
            isDay = false;
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
        return isDay;
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
            CheckForDayNightTransition();
            GameEvents.current.HourUpdated(currentHour);
        }

        // Check for hour overflow and increment days accordingly.
        if (currentHour >= 24)
        {
            currentDay++;
            currentHour %= 24;

            GameEvents.current.DayUpdated(currentDay);
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
        
        CalculateAndUpdateSunlight();
    }

    private bool dayStarted = false;
    private bool nightStarted = false;
    private void CheckForDayNightTransition()
    {
        if (currentHour == dayStartHour && !dayStarted)
        {
            GameEvents.current.DayStarted();
            isDay = true;
            dayStarted = true; // Mark the day start transition as handled
            nightStarted = false;
            _audioEmitter.PlayClipFromCategory("DayTime", false);
        }
        else if (currentHour == nightStartHour && !nightStarted)
        {
            GameEvents.current.NightStarted(currentDay != 0 && currentDay % redMoonNightInterval == 0);
            isDay = false;
            nightStarted = true; // Mark the night start transition as handled
            dayStarted = false;
            _audioEmitter.PlayClipFromCategory("NightTime", false);
        }
    }

    private void UpdateTimeUI()
    {
        string formattedHour = currentHour.ToString("D2");
        string formattedMinute = ((int)currentMinute).ToString("D2");
        //timeText.text = $"{formattedHour}:{formattedMinute}";
        //calendarText.text = $"{currentDay+1}";
    }
    public float GetCurrentTime() => currentHour + currentMinute / 60f;
    public void CalculateAndUpdateSunlight()
    {
        float currentHour = GetCurrentTime();
        int sunlightLevel = Mathf.RoundToInt(CalculateSunlightLevel(currentHour));

        if (sunlightLevel != lastSunlightLevelUpdate)
        {
            LightGenerator.Instance.UpdateSunlightBrightness(sunlightLevel);
            // LightningControl.Instance.UpdateSunlightBrightness(sunlightLevel);
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
    
    public int GetCurrentHour()
    {
        
        return currentHour;
    }

    public int GetCurrentDay()
    {
        return currentDay;
    }

    

}
