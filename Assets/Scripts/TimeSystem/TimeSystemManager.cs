using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeSystemManager : MonoBehaviour
{
    public float dayToRealTimeInSecond;
    public int dayStartHour = 6;
    public int nightStartHour = 20;

    public Action OnDayStartedHandler;
    public Action OnNightStartedHandler;
    public Action<int> OnHourUpdatedHandler;
    public Action<int> OnDayUpdatedHandler;

    private GameObject timeText; //x天x时
    private int currentHour = 0;
    private int currentDay = 0;
    // Start is called before the first frame update
    void Start()
    {
        timeText = GameObject.Find(Constants.Name.TIME_TEXT);
        if (currentHour >= dayStartHour && currentHour < nightStartHour)
        {
            OnDayStartedHandler?.Invoke();
        }
        else
        {
            OnNightStartedHandler?.Invoke();
        }
        StartCoroutine(UpdateTime());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetDayTimeLengthInHour()
    {
        return nightStartHour - dayStartHour;
    }

    public int GetMidDayTime()
    {
        return (nightStartHour + dayStartHour) / 2;
    }

    private IEnumerator UpdateTime()
    {
        while(true)
        {
            yield return new WaitForSeconds(dayToRealTimeInSecond / 24);
            currentHour += 1;
            OnHourUpdatedHandler?.Invoke(currentHour);
            if (currentHour == 24)
            {
                currentDay += 1;
                currentHour = 0;
                OnDayUpdatedHandler?.Invoke(currentDay);
            }
            UpdateTimeUI();

            if (currentHour == dayStartHour)
            {
                OnDayStartedHandler?.Invoke();
            }
            else if (currentHour == nightStartHour)
            {
                OnNightStartedHandler?.Invoke();
            }
        }
        
    }

    private void UpdateTimeUI()
    {
        timeText.GetComponent<TMP_Text>().text = currentDay + "天" + currentHour + "时";
    }
}
