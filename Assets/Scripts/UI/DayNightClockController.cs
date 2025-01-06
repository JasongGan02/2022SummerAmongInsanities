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
    private float frameInterval; // Time per frame in real seconds
    private float frameTimer = 0f; // Timer to track frame updates
    private int currentFrame = 0; // Tracks the current sprite frame

    private void Start()
    {
        dayClockImage = dayClockObject.GetComponent<Image>();
        nightClockImage = nightClockObject.GetComponent<Image>();

        if (dayClockImage == null || nightClockImage == null)
        {
            Debug.LogError("❌ Day and Night Clock objects must have an Image component.");
        }

        CalculateFrameInterval();

        // Initialize day or night mode
        UpdateClockState();
    }

    private void CalculateFrameInterval()
    {
        float dayDuration = timeSystemManager.dayToRealTimeInSecond;
        if (dayDuration <= 0)
        {
            Debug.LogError("❌ dayToRealTimeInSecond must be set to a positive value in TimeSystemManager.");
            dayDuration = 1200f; // Default fallback (20 min per day)
        }

        frameInterval = dayDuration / 24f; // 12 frames per full day-night cycle
        Debug.Log($"⏳ Frame Interval Set to {frameInterval} seconds per frame.");
    }

    private void Update()
    {
        UpdateClockFrame();
    }

    private void UpdateClockFrame()
    {
        // Update frame timer
        frameTimer += Time.deltaTime;

        if (frameTimer >= frameInterval)
        {
            frameTimer = 0f; // Reset timer
            currentFrame = (currentFrame + 1) % 12; // Loop through frames (0–11)

            Debug.Log($"🕒 Clock Frame Updated: {currentFrame}");

            if (isDay)
            {
                dayClockImage.sprite = dayFrames[currentFrame];
            }
            else
            {
                nightClockImage.sprite = nightFrames[currentFrame];
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
        currentFrame = 0;
        frameTimer = 0;
        Debug.Log("🌞 Switched to Day Mode");
    }

    /// <summary>
    /// Handles switching to night mode.
    /// </summary>
    private void OnNightStarted(bool isRedMoon)
    {
        isDay = false;
        dayClockObject.SetActive(false);
        nightClockObject.SetActive(true);
        currentFrame = 0;
        frameTimer = 0;
        Debug.Log($"🌙 Switched to Night Mode | Is Red Moon: {isRedMoon}");
    }

    /// <summary>
    /// Ensures the initial state matches the current time.
    /// </summary>
    private void UpdateClockState()
    {
        if (timeSystemManager.IsInDaytime())
        {
            OnDayStarted();
        }
        else
        {
            OnNightStarted(timeSystemManager.GetCurrentDay() % timeSystemManager.redMoonNightInterval == 0);
        }
    }

    private void OnDestroy()
    {
        GameEvents.current.OnDayStarted -= OnDayStarted;
        GameEvents.current.OnNightStarted -= OnNightStarted;
    }
}
