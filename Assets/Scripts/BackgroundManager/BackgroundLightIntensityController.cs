using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BackgroundLightIntensityController : MonoBehaviour
{
    private TimeSystemManager timeSystemManager;
    private Light2D backgroundLight;
    private float transitionTimeInGameHour = 3;
    private float transitionTimeInRealSecond;
    private float timeCounter = 0;

    private bool sunrise = false;
    private bool sunset = false;

    GameObject Player;
    // Start is called before the first frame update
    void Awake()
    {
        timeSystemManager = FindObjectOfType<TimeSystemManager>();
        backgroundLight = GetComponent<Light2D>();
        transitionTimeInRealSecond = timeSystemManager.dayToRealTimeInSecond / 24 * transitionTimeInGameHour;
        //backgroundLight.intensity = 0.2f;
    }

    private void OnEnable()
    {
        timeSystemManager.OnHourUpdatedHandler += UpdateDaylightStatus;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeSystemManager.isDebugDayTime)
        {
            backgroundLight.intensity = 1; // or whatever daytime intensity you prefer
            return;
        }
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }
        else
        {
            transform.position = new Vector2(Player.transform.position.x, transform.position.y);
        }

        timeCounter += Time.deltaTime / transitionTimeInRealSecond;
        if (sunrise)
        {
            backgroundLight.intensity = Mathf.Lerp(0.2f, 1, timeCounter);
        }
        else if (sunset)
        {
            backgroundLight.intensity = Mathf.Lerp(1, 0.2f, timeCounter);
        }
    }

    private void UpdateDaylightStatus(int hour)
    {
        if (hour == timeSystemManager.dayStartHour)
        {
            sunrise = true;
            timeCounter = 0;
        }
        else if (hour == timeSystemManager.dayStartHour + transitionTimeInGameHour)
        {
            sunrise = false;
        }
        else if (hour == timeSystemManager.nightStartHour)
        {
            sunset = true;
            timeCounter = 0;
        }
        else if (hour == timeSystemManager.nightStartHour + transitionTimeInGameHour)
        {
            sunset = false;
        }
    }
}
