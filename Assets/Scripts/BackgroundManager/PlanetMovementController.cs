using System.Collections;
using UnityEngine;

public class PlanetMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float heightOffset = 60f;
    [SerializeField] private float height = 7f;
    [SerializeField] private float radius = 13f;

    private float duration;
    private float speed;
    private bool isInitialized = false;

    private Camera mainCamera;
    private TimeSystemManager timeSystemManager;
    private WorldGenerator worldGenerator;

    // Start the movement at an angle of -π/2 (so that motion starts from the bottom)
    private float timeCounter = -Mathf.PI / 2;

    private void Start()
    {
        mainCamera = Camera.main;
        // Use a coroutine to wait for TimeSystemManager to be assigned.
        StartCoroutine(WaitForTimeSystemManagerAndInitialize());
    }

    /// <summary>
    /// Waits until TimeSystemManager.Instance is available, then assigns dependencies and initializes.
    /// </summary>
    private IEnumerator WaitForTimeSystemManagerAndInitialize()
    {
        // Wait until TimeSystemManager's instance is assigned.
        yield return new WaitUntil(() => TimeSystemManager.Instance != null);

        // Now it's safe to assign our dependency.
        timeSystemManager = TimeSystemManager.Instance;

        worldGenerator = FindObjectOfType<WorldGenerator>();
        if (worldGenerator != null && worldGenerator.settings != null && worldGenerator.settings.Length > 0)
        {
            heightOffset = worldGenerator.settings[0].heightAddition +
                           worldGenerator.settings[0].heightMultiplier * 0.6f;
        }
        else
        {
            Debug.LogWarning("WorldGenerator or its settings not found. Using default heightOffset.");
        }

        Initialize();
    }

    /// <summary>
    /// Initializes the planet movement parameters based on the current time.
    /// </summary>
    private void Initialize()
    {
        // Make sure TimeSystemManager is available.
        if (timeSystemManager == null)
        {
            Debug.LogError("TimeSystemManager is null. Cannot initialize PlanetMovementController.");
            return;
        }

        // Adjust the starting angle based on the percentage of night that has passed.
        timeCounter = -Mathf.PI / 2 + Mathf.PI * timeSystemManager.GetHowMuchPercentageOfNightTimeHasPassed();

        // Determine duration based on whether it's day or night.
        if (timeSystemManager.IsDayTime())
        {
            duration = timeSystemManager.GetDayTimeLengthInHour();
        }
        else
        {
            duration = 24 - timeSystemManager.GetDayTimeLengthInHour();
        }

        // Convert duration from hours into real-time seconds.
        duration = (duration / 24f) * timeSystemManager.dayToRealTimeInSecond;
        speed = Mathf.PI / duration;

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized)
        {
            // Skip update until initialization is complete.
            return;
        }

        // Update the time counter based on the calculated speed.
        timeCounter += Time.deltaTime * speed;

        // Calculate new positions using sine and cosine.
        float x = Mathf.Sin(timeCounter) * radius;
        float y = Mathf.Cos(timeCounter) * height;

        // Ensure mainCamera is available.
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Update the planet's position relative to the camera.
        transform.position = new Vector2(x + mainCamera.transform.position.x, y + heightOffset);
    }
}