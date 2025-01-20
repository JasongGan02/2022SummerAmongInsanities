using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.Rendering.Universal;

public class LightningControl : MonoBehaviour
{
    public static LightningControl Instance { get; private set; }
    private float sunlightBrightness = 15f;
    private float lightIntensity = 1f;
    public Light2D EnvironmentLight;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }

    public void UpdateSunlightBrightness(float newSunlightBrightness)
    {
        sunlightBrightness = newSunlightBrightness;
        UpdateLightIntensity();
    }

    public void UpdateLightIntensity()
    {
        lightIntensity = Map(sunlightBrightness, 4, 15, 0.3f, 1f);
        EnvironmentLight.intensity = lightIntensity;
    }

    // Utility method for mapping ranges
    private float Map(float value, float oldMin, float oldMax, float newMin, float newMax)
    {
        return (newMax - newMin) * (value - oldMin) / (oldMax - oldMin) + newMin;
    }
}
