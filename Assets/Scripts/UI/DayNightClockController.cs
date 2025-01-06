using UnityEngine;
using UnityEngine.UI;

public class DayNightClockController : MonoBehaviour
{
    [Header("Time System Reference")]
    [SerializeField] private TimeSystemManager timeSystemManager;

    [Header("Day and Night Objects")]
    [SerializeField] private GameObject dayClockObject; // Day clock GameObject
    [SerializeField] private GameObject nightClockObject; // Night clock GameObject

    [Header("Day and Night Sprites")]
    [SerializeField] private Sprite[] dayFrames;   // 12 day sprites
    [SerializeField] private Sprite[] nightFrames; // 12 night sprites

    private Image dayClockImage;
    private Image nightClockImage;

    private bool isDay = true;
    private int lastUpdatedHour = -1;

    private void Start()
    {
        dayClockImage = dayClockObject.GetComponent<Image>();
        nightClockImage = nightClockObject.GetComponent<Image>();

        if (dayClockImage == null || nightClockImage == null)
        {
            throw new MissingComponentException("Day and Night Clock objects must have an Image component.");
        }

        // Subscribe to day/night events
        GameEvents.current.OnDayStarted += OnDayStarted;
        GameEvents.current.OnNightStarted += OnNightStarted;

        // Initialize day or night mode
        UpdateClockState();
        UpdateClockFrame();
    }

    private void Update()
    {
        int currentHour = Mathf.FloorToInt(timeSystemManager.GetCurrentTime());

        if (currentHour != lastUpdatedHour)
        {
            lastUpdatedHour = currentHour;
            UpdateClockFrame();
            UpdateClockState();
        }
    }

    /// <summary>
    /// Updates the current frame of the clock based on the hour and time system settings.
    /// </summary>
    private void UpdateClockFrame()
    {
        int currentHour = Mathf.FloorToInt(timeSystemManager.GetCurrentTime());
        int frameIndex;

        if (isDay)
        {
            // Map day hours to day clock frames
            frameIndex = (currentHour - timeSystemManager.dayStartHour + 24);
        }
        else
        {
            // Map night hours to night clock frames
            frameIndex = (currentHour - timeSystemManager.nightStartHour + 24);
        }

        frameIndex = Mathf.Clamp(frameIndex, 0, 11);

        if (isDay)
        {
            dayClockImage.sprite = dayFrames[frameIndex];
        }
        else
        {
            nightClockImage.sprite = nightFrames[frameIndex];
        }
    }

    /// <summary>
    /// Updates the day or night state based on the hour.
    /// </summary>
    private void UpdateClockState()
    {
        int currentHour = Mathf.FloorToInt(timeSystemManager.GetCurrentTime());

        if (currentHour >= timeSystemManager.dayStartHour && currentHour < timeSystemManager.nightStartHour)
        {
            if (!isDay)
            {
                OnDayStarted();
            }
        }
        else
        {
            if (isDay)
            {
                OnNightStarted(false);
            }
        }
    }

    /// <summary>
    /// Handles switching to day mode.
    /// </summary>
    private void OnDayStarted()
    {
        isDay = true;
        dayClockObject.SetActive(true);
        nightClockObject.SetActive(false);
    }

    /// <summary>
    /// Handles switching to night mode.
    /// </summary>
    private void OnNightStarted(bool isRedMoon)
    {
        isDay = false;
        dayClockObject.SetActive(false);
        nightClockObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (GameEvents.current != null)
        {
            GameEvents.current.OnDayStarted -= OnDayStarted;
            GameEvents.current.OnNightStarted -= OnNightStarted;
        }
    }
}
