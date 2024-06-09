using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private GameObject background;
    private GameObject sun;
    private GameObject moon;
    private GameObject redMoon;
    private GameObject backgroundLight;

    private TimeSystemManager timeSystemManager;
    private WorldGenerator worldGenerator;
    private float heightOffset;
    // Start is called before the first frame update
    void Awake()
    {
        GameObject BG = background.transform.Find(Constants.Name.BACKGROUND).gameObject;
       
        sun = BG.transform.Find(Constants.Name.SUN).gameObject;
        moon = BG.transform.Find(Constants.Name.MOON).gameObject;
        redMoon = BG.transform.Find(Constants.Name.RED_MOON).gameObject;
        backgroundLight = background.transform.Find(Constants.Name.BACKGROUND_LIGHT).gameObject;
        timeSystemManager = FindObjectOfType<TimeSystemManager>();
        worldGenerator = FindObjectOfType<WorldGenerator>();

        BG.transform.position = new Vector3(BG.transform.position.x, worldGenerator.settings[0].heightAddition + worldGenerator.settings[0].heightMultiplier * 0.6f, BG.transform.position.z);
        GameEvents.current.OnDayStarted += OnDayStarted;
        GameEvents.current.OnNightStarted += OnNightStarted;

    }
    
    

    private void OnDestroy()
    {
        GameEvents.current.OnDayStarted -= OnDayStarted;
        GameEvents.current.OnNightStarted -= OnNightStarted;
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
