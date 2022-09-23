using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    private GameObject sun;
    private GameObject dayBackground;
    private GameObject nightBackground;

    private TimeSystemManager timeSystemManager;
    // Start is called before the first frame update
    void Awake()
    {
        dayBackground = GameObject.Find(Constants.Name.BG);
        nightBackground = GameObject.Find(Constants.Name.NIGHT_BACKGROUND);
        sun = GameObject.Find(Constants.Name.SUN);
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
        dayBackground.SetActive(true);
        sun.SetActive(true);
        nightBackground.SetActive(false);
    }

    private void OnNightStarted()
    {
        dayBackground.SetActive(false);
        sun.SetActive(false);
        nightBackground.SetActive(true);
    }
}
