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
    
    private int currentFrame = 0; // Tracks the current sprite frame
    private bool isDay;

    private void Start()
    {
        dayClockImage = dayClockObject.GetComponent<Image>();
        nightClockImage = nightClockObject.GetComponent<Image>();

        if (dayClockImage == null || nightClockImage == null)
        {
            Debug.LogError("\u274C Day and Night Clock objects must have an Image component.");
            return;
        }


        // Subscribe to events
        GameEvents.current.OnHourUpdated += UpdateFrameToCurrentHour;
        GameEvents.current.OnDayStarted += OnDayStarted;
        GameEvents.current.OnNightStarted += OnNightStarted;

        InitializeClockState();
    }
    
    private void InitializeClockState()
    {
        // Determine if the current time is daytime or nighttime
        if (timeSystemManager.IsInDaytime())
        {
            OnDayStarted();
        }
        else
        {
            OnNightStarted(timeSystemManager.GetCurrentDay() % timeSystemManager.redMoonNightInterval == 0);
        }

        // Set the initial frame based on the current time progress
        UpdateFrameToCurrentHour(timeSystemManager.GetCurrentHour());
    }

    private void OnDayStarted()
    {
        isDay = true;
        dayClockObject.SetActive(true);
        nightClockObject.SetActive(false);
    }
    
    private void OnNightStarted(bool isBloodMoon)
    {
        isDay = false;
        dayClockObject.SetActive(false);
        nightClockObject.SetActive(true);
    }

    private void UpdateFrameToCurrentHour(int currentHour)
    {
        if (isDay)
        {
            // Correctly map currentHour to day frame
            currentFrame = ((currentHour - timeSystemManager.dayStartHour + 12) % 12 + 12) % 12;
            dayClockImage.sprite = dayFrames[Mathf.Clamp(currentFrame, 0, 11)];
        }
        else
        {
            // Correctly map currentHour to night frame
            currentFrame = ((currentHour - timeSystemManager.nightStartHour + 12) % 12 + 12) % 12;
            nightClockImage.sprite = nightFrames[Mathf.Clamp(currentFrame, 0, 11)];
        }
    }


    private void OnDestroy()
    {
        GameEvents.current.OnHourUpdated -= UpdateFrameToCurrentHour;
        GameEvents.current.OnDayStarted -= OnDayStarted;
        GameEvents.current.OnNightStarted -= OnNightStarted;
    }
}
