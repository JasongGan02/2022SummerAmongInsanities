using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BackgroundManager : MonoBehaviour
{
    private GameObject sun;
    private GameObject redMoon;
    private GameObject backgroundLight;

    private TimeSystemManager timeSystemManager;
    // Start is called before the first frame update
    void Awake()
    {
        sun = GameObject.Find(Constants.Name.SUN);
        redMoon = GameObject.Find(Constants.Name.RED_MOON);
        backgroundLight = GameObject.Find(Constants.Name.BACKGROUND_LIGHT);
        timeSystemManager = FindObjectOfType<TimeSystemManager>();

        timeSystemManager.OnDayStartedHandler += OnDayStarted;
        timeSystemManager.OnNightStartedHandler += OnNightStarted;
        timeSystemManager.OnRedMoonNightStartedHandler += OnRedMoonNightStarted;
        timeSystemManager.OnRedMoonNightEndedHandler += OnRedMoonNightEnded;
    }

    private void OnDisable()
    {
        timeSystemManager.OnDayStartedHandler -= OnDayStarted;
        timeSystemManager.OnNightStartedHandler -= OnNightStarted;
        timeSystemManager.OnRedMoonNightStartedHandler -= OnRedMoonNightStarted;
        timeSystemManager.OnRedMoonNightEndedHandler -= OnRedMoonNightEnded;
    }

    private void OnDayStarted()
    {
        //backgroundLight.GetComponent<Light2D>().intensity = 1f;
        sun.SetActive(true);
    }

    private void OnNightStarted()
    {
        //backgroundLight.GetComponent<Light2D>().intensity = 0.2f;
        sun.SetActive(false);
    }

    private void OnRedMoonNightStarted()
    {
        redMoon.SetActive(true);
    }

    private void OnRedMoonNightEnded()
    {
        redMoon.SetActive(false);
    }
}
