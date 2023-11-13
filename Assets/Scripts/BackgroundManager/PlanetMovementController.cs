using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetMovementController : MonoBehaviour
{
    [SerializeField] private float heightOffset = 65f;
    [SerializeField] private float height = 8f;
    [SerializeField] private float radius = 13f;

    private Camera mainCamera;
    private TimeSystemManager timeSystemManager;

    private void Awake()
    {
        mainCamera = Camera.main;
        timeSystemManager = FindObjectOfType<TimeSystemManager>();
    }

    private void Update()
    {
        // Calculate the position based on the percentage of night time that has passed
        float nightTimePercentage = timeSystemManager.GetHowMuchPercentageOfNightTimeHasPassed();

        // Assuming the night starts with timeCounter = -Mathf.PI / 2 and ends at 3 * Mathf.PI / 2
        float timeCounter = Mathf.PI * 2 * nightTimePercentage - Mathf.PI / 2;

        var x = Mathf.Sin(timeCounter) * radius;
        var y = Mathf.Cos(timeCounter) * height;

        // Update the planet's position based on the night cycle
        transform.position = new Vector3(x + mainCamera.transform.position.x, y + heightOffset, transform.position.z);
    }
}
