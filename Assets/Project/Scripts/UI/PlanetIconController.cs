using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetIconController : MonoBehaviour
{
    [SerializeField] private Sprite[] iconList; // 0 = sun, 1 = moon, 2 = redmoon

    private Image icon;

    // Start is called before the first frame update
    void Start()
    {
        icon = GetComponent<Image>();
        GameEvents.current.OnDayStarted += ChangeToSun;
        GameEvents.current.OnNightStarted += ChangeToMoon;
    }

    private void ChangeToSun()
    {
        icon.sprite = iconList[0];
    }

    private void ChangeToMoon(bool isRedMoon)
    {
        //Debug.Log("Change to MOON IS :" + isRedMoon);
        if (isRedMoon)
        {
            icon.sprite = iconList[2];
        }
        else
        {
            icon.sprite = iconList[1];
        }
    }

    // Make sure to unsubscribe from the events when the object is destroyed
    private void OnDestroy()
    {
        GameEvents.current.OnDayStarted -= ChangeToSun;
        GameEvents.current.OnNightStarted -= ChangeToMoon;
    }
}