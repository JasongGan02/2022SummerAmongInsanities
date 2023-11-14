using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetMovementController : MonoBehaviour
{
    [SerializeField] private float heightOffset = 60f;
    [SerializeField] private float height = 7f;
    [SerializeField] private float radius = 13f;
    private float duration;
    private float speed;

    private Camera mainCamera;
    private TimeSystemManager timeSystemManager;

    private float timeCounter = -Mathf.PI / 2;
    private void Awake()
    {
        mainCamera = Camera.main;
        timeSystemManager = FindObjectOfType<TimeSystemManager>();
        if (timeSystemManager.IsInDaytime() ) 
        {
            duration = timeSystemManager.GetDayTimeLengthInHour();
        }
        else
        {
            duration = 24 - timeSystemManager.GetDayTimeLengthInHour() - 1;
        }
        
        speed = Mathf.PI / duration;
    }

    private void OnEnable()
    {
        timeCounter = -Mathf.PI / 2 + Mathf.PI * timeSystemManager.GetHowMuchPercentageOfNightTimeHasPassed();
    }

    private void Update()
    {
        timeCounter += Time.deltaTime * speed;
        var x = Mathf.Sin(timeCounter) * radius;
        var y = Mathf.Cos(timeCounter) * height;
        transform.localPosition = new Vector2(x + mainCamera.transform.position.x, y + heightOffset);
    }
}