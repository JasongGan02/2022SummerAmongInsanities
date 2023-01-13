using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BackgroundManager : MonoBehaviour
{
    private GameObject sun;
    private GameObject moon;
    private GameObject redMoon;
    private GameObject backgroundLight;

    private TimeSystemManager timeSystemManager;
    // Start is called before the first frame update
    void Awake()
    {
        GameObject background = GameObject.Find(Constants.Name.BACKGROUND);
        sun = background.transform.Find(Constants.Name.SUN).gameObject;
        moon = background.transform.Find(Constants.Name.MOON).gameObject;
        redMoon = background.transform.Find(Constants.Name.RED_MOON).gameObject;
        backgroundLight = background.transform.Find(Constants.Name.BACKGROUND_LIGHT).gameObject;
        timeSystemManager = FindObjectOfType<TimeSystemManager>();

        timeSystemManager.OnDayStartedHandler += OnDayStarted;
        timeSystemManager.OnNightStartedHandler += OnNightStarted;
    }

    private void OnDisable()
    {
        timeSystemManager.OnDayStartedHandler -= OnDayStarted;
        timeSystemManager.OnNightStartedHandler -= OnNightStarted;
    }

    private void OnDayStarted()
    {
        sun.SetActive(true);
        redMoon.SetActive(false);
        moon.SetActive(false);
    }

    private void OnNightStarted(bool isRedMoonNight)
    {
        sun.SetActive(false);

        if (isRedMoonNight)
        {
            redMoon.SetActive(true);
            moon.SetActive(false);
        }
        else
        {
            moon.SetActive(true);
            redMoon.SetActive(false);
        }
    }
}
