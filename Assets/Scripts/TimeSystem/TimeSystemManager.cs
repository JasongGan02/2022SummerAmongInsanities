using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    public Action OnDayStartedHandler;
    public Action OnDuskStartedHandler;
    public Action<bool> OnNightStartedHandler; // The boolean is used to set moon type

    public Action<int> OnHourUpdatedHandler;
    public Action<int> OnDayUpdatedHandler;

    public audioManager am;

    private GameObject timeText; //x��xʱ
    private int currentMinute = 0;
    private int currentHour = 0;
    private int currentDay = 0;
    
    //LightManager variables
    private float lastSunlightLevelUpdate = -1f;
    private float maxSunlightLevel = 15; 
    [SerializeField] float minSunlightLevel = 4;
    // Start is called before the first frame update
    void Awake()
    {

        am = GameObject.FindGameObjectWithTag("audio").GetComponent<audioManager>();
        timeText = GameObject.Find(Constants.Name.TIME_TEXT);

        // If debug mode is enabled, set time to daytime
        if (isDebugDayTime)
        {
            currentHour = dayStartHour+1;
            OnDayStartedHandler?.Invoke();
            am.playBGM(am.DayTime);
        }
        else
        {
            am.playBGM(am.NightTime);
            if (currentHour >= dayStartHour && currentHour < nightStartHour)
            {
                OnDayStartedHandler?.Invoke();
            }
            else
            {
                OnNightStartedHandler?.Invoke(false);
            }
        }

        StartCoroutine(UpdateTime());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public int GetDayTimeLengthInHour()
    {
        return nightStartHour - dayStartHour;
    }

    public int GetMidDayTime()
    {
        return (nightStartHour + dayStartHour) / 2;
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

    public float GetCurTime() 
    {
        return currentHour + currentMinute / 60f; // 1 hour 30 minutes = 1.5 hours
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

    private IEnumerator UpdateTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(dayToRealTimeInSecond / (24 * 60));

            // Skip updating the time if debug mode is on
            if (!isDebugDayTime)
            {
                currentMinute++;
                if (currentMinute == 60)
                {
                    currentHour++;
                    currentMinute = 0;

                    OnHourUpdatedHandler?.Invoke(currentHour);
                }

                if (currentHour == 24)
                {
                    currentDay++;
                    currentHour = 0;
                    OnDayUpdatedHandler?.Invoke(currentDay);
                }

                //OnMinuteUpdatedHandler?.Invoke(currentHour, currentMinute); // Notify listeners every minute
            }
            else
            {
                currentHour = dayStartHour;
                currentMinute = 0; // Reset minutes in debug mode
            }

            if (currentHour == 24)
            {
                
                currentDay += 1;
                currentHour = 0;
                OnDayUpdatedHandler?.Invoke(currentDay);
            }

            UpdateTimeUI();

            CheckForDayNightTransition();
            CalculateAndUpdateSunlight();
        }
    }

    private void CheckForDayNightTransition()
    {
        if (currentHour == dayStartHour && currentMinute == 0)
        {
            OnDayStartedHandler?.Invoke();
        }
        else if (currentHour == nightStartHour && currentMinute == 0)
        {
            if (!isDebugDayTime)
            {
                if (currentDay % redMoonNightInterval == 0)
                OnDayStartedHandler?.Invoke();
                am.playBGM(am.DayTime);
            }
            else if (currentHour == nightStartHour)
            {
                am.playBGM(am.NightTime);
                if (!isDebugDayTime)
                {
                    OnNightStartedHandler?.Invoke(true);
                }
                else
                {
                    OnNightStartedHandler?.Invoke(false);
                }
            }
        }
    }

    private void UpdateTimeUI()
    {
        timeText.GetComponent<TMP_Text>().text = $"{currentDay}天{currentHour}时{currentMinute}分";
    }
    
    public void CalculateAndUpdateSunlight()
    {
        float currentHour = GetCurTime(); // This should now return a more precise time, including minutes as a fraction
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

        if (!Mathf.Approximately(sunlightLevel, lastSunlightLevelUpdate))
        {
            LightGenerator.Instance.UpdateSunlightBrightness(sunlightLevel);
            lastSunlightLevelUpdate = sunlightLevel;
        }
    }

}
