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
    private WorldGenerator worldGenerator;

    private float timeCounter = -Mathf.PI / 2;
    private void Awake()
    {
        mainCamera = Camera.main;
        timeSystemManager = FindObjectOfType<TimeSystemManager>();
        worldGenerator = FindObjectOfType<WorldGenerator>();
        heightOffset = worldGenerator.settings[0].heightAddition + worldGenerator.settings[0].heightMultiplier * 0.6f;
    }

    private void OnEnable()
    {
        
        timeCounter = -Mathf.PI / 2 + Mathf.PI * timeSystemManager.GetHowMuchPercentageOfNightTimeHasPassed();
        if (timeSystemManager.IsInDaytime())
        {
            duration = timeSystemManager.GetDayTimeLengthInHour();
        }
        else
        {
            duration = 24 - timeSystemManager.GetDayTimeLengthInHour();
        }
        duration *= 0.85f;
        speed = Mathf.PI / duration;
    }

    private void Update()
    {
        timeCounter += Time.deltaTime * speed;
        var x = Mathf.Sin(timeCounter) * radius;
        var y = Mathf.Cos(timeCounter) * height;
        transform.position = new Vector2(x + mainCamera.transform.position.x, y + heightOffset);
    }
}