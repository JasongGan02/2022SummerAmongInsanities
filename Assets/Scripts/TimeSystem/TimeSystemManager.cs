using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeSystemManager : MonoBehaviour
{
    #region Singleton

    /// <summary>
    /// Singleton instance for global access.
    /// </summary>
    public static TimeSystemManager Instance { get; private set; }

    private void Awake()
    {
        // Implement the singleton pattern: If another instance exists, destroy this one.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Initialize calculated values and required components.
        gameMinuteInRealSec = (24f * 60f) / dayToRealTimeInSecond;
        audioEmitter = GetComponent<AudioEmitter>();

        // Initialize starting time.
        currentHour = firstDayStartHour;
        currentMinute = firstDayStartMinute;
    }

    #endregion

    #region Inspector Fields

    [Header("Time Settings")]
    [Tooltip("Length of a full in-game day in real seconds.")]
    public float dayToRealTimeInSecond = 120f;
    [Tooltip("Hour at which the day starts.")]
    public int dayStartHour = 8;
    [Tooltip("Hour at which the night starts.")]
    public int nightStartHour = 20;
    [Tooltip("Every N-th night, a red moon event occurs.")]
    public int redMoonNightInterval = 5;

    [Header("Dawn and Dusk Settings")]
    [Tooltip("Hour when dawn starts.")]
    public int dawnStartHour = 5;
    [Tooltip("Hour when dawn ends.")]
    public int dawnEndHour = 7;
    [Tooltip("Hour when dusk starts.")]
    public int duskStartHour = 18;
    [Tooltip("Hour when dusk ends.")]
    public int duskEndHour = 20;

    [Header("Initial Day Time Settings")]
    [Tooltip("Initial hour for the first day.")]
    public int firstDayStartHour = 8;
    [Tooltip("Initial minute for the first day.")]
    public int firstDayStartMinute = 0;

    [Header("Debug Settings")]
    [Tooltip("Force the time system to start in daytime.")]
    public bool isDebugDayTime = false;

    #endregion

    #region Private Fields

    // Time tracking state.
    private float currentMinute = 0f;
    private int currentHour = 0;
    private int currentDay = 0;

    // Light/Sunlight management.
    private int lastSunlightLevelUpdate = -1;
    private readonly float maxSunlightLevel = 15f;
    private readonly float minSunlightLevel = 4f;

    // The number of in-game minutes advanced per real-time second.
    private float gameMinuteInRealSec;

    // Audio emitter for playing time-of-day sounds.
    private AudioEmitter audioEmitter;

    // State flags to prevent multiple event triggers.
    private bool dayStarted = false;
    private bool nightStarted = false;

    // Flag to indicate if it's currently day.
    private bool isDay;

    #endregion

    #region Public Properties

    /// <summary>
    /// Returns whether the current night is a red moon night.
    /// This is computed based on the current day and the red moon interval.
    /// </summary>
    public bool IsRedMoon
    {
        get
        {
            int effectiveDay = currentDay;
            // If it is nighttime and the current hour is before the defined dayStartHour,
            // then the effective day is still the previous day.
            if (currentHour >= 0 && currentHour < dayStartHour)
            {
                effectiveDay = Mathf.Max(0, currentDay - 1);
            }

            bool isRed = effectiveDay != 0 && effectiveDay % redMoonNightInterval == 0 && !isDay;
            Debug.Log($"[TimeSystemManager] IsRedMoon computed: {isRed} (EffectiveDay: {effectiveDay}, CurrentDay: {currentDay}, " +
                      $"RedMoonInterval: {redMoonNightInterval}, IsDayTime: {IsDayTime()}, CurrentHour: {currentHour})");
            return isRed;
        }
    }

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (isDebugDayTime)
        {
            SetToDaytime();
        }
        else
        {
            InitializeTimeBasedOnCurrentHour();
        }
    }

    private void Update()
    {
        // Advance game time based on the frame's delta time.
        float gameTimeAdvancePerFrame = gameMinuteInRealSec * Time.deltaTime;
        AdvanceGameTime(gameTimeAdvancePerFrame);
    }

    #endregion

    #region Time Initialization and Transition Methods

    /// <summary>
    /// Sets the time system to daytime immediately.
    /// </summary>
    private void SetToDaytime()
    {
        currentHour = dayStartHour + 1;
        GameEvents.current.DayStarted();
        isDay = true;
        dayStarted = true;
        nightStarted = false;
        audioEmitter.PlayClipFromCategory("DayTime", false);
    }

    /// <summary>
    /// Initializes time and triggers the appropriate events based on the current hour.
    /// </summary>
    private void InitializeTimeBasedOnCurrentHour()
    {
        if (currentHour >= dayStartHour && currentHour < nightStartHour)
        {
            audioEmitter.PlayClipFromCategory("DayTime", false);
            GameEvents.current.DayStarted();
            isDay = true;
            dayStarted = true;
            nightStarted = false;
            GameEvents.current.HourUpdated(currentHour);
        }
        else
        {
            // Use the computed property for red moon status.
            isDay = false;
            nightStarted = true;
            dayStarted = false;
            GameEvents.current.NightStarted(IsRedMoon);
            GameEvents.current.HourUpdated(currentHour);
        }
    }

    /// <summary>
    /// Increments the in-game time by the specified minutes and handles overflow.
    /// </summary>
    /// <param name="gameMinutesToAdvance">Number of in-game minutes to advance.</param>
    private void IncrementTime(float gameMinutesToAdvance)
    {
        currentMinute += gameMinutesToAdvance;

        // Handle minute overflow.
        if (currentMinute >= 60f)
        {
            int additionalHours = (int)(currentMinute / 60);
            currentHour += additionalHours;
            currentMinute %= 60f;
            CheckForDayNightTransition();
            GameEvents.current.HourUpdated(currentHour);
        }

        // Handle hour overflow into the next day.
        if (currentHour >= 24)
        {
            currentDay++;
            currentHour %= 24;
            GameEvents.current.DayUpdated(currentDay);
        }
    }

    /// <summary>
    /// Checks for transitions between day and night and triggers associated events.
    /// </summary>
    private void CheckForDayNightTransition()
    {
        if (currentHour == dayStartHour && !dayStarted)
        {
            GameEvents.current.DayStarted();
            isDay = true;
            dayStarted = true;
            nightStarted = false;
            audioEmitter.PlayClipFromCategory("DayTime", false);
        }
        else if (currentHour == nightStartHour && !nightStarted)
        {
            // Use the computed property for red moon status.
            isDay = false;
            nightStarted = true;
            dayStarted = false;
            GameEvents.current.NightStarted(IsRedMoon);
            audioEmitter.PlayClipFromCategory("NightTime", false);
        }
    }

    #endregion

    #region Update Methods

    /// <summary>
    /// Advances the game time and updates sunlight accordingly.
    /// </summary>
    /// <param name="gameMinutesToAdvance">The number of in-game minutes to advance.</param>
    private void AdvanceGameTime(float gameMinutesToAdvance)
    {
        if (!isDebugDayTime)
        {
            IncrementTime(gameMinutesToAdvance);
        }

        CalculateAndUpdateSunlight();
    }

    /// <summary>
    /// Calculates and updates the sunlight level based on the current time.
    /// </summary>
    public void CalculateAndUpdateSunlight()
    {
        float timeOfDay = GetCurrentTime();
        int sunlightLevel = Mathf.RoundToInt(CalculateSunlightLevel(timeOfDay));

        if (sunlightLevel != lastSunlightLevelUpdate)
        {
            LightGenerator.Instance.UpdateSunlightBrightness(sunlightLevel);
            // Optionally update other lighting systems:
            // LightningControl.Instance.UpdateSunlightBrightness(sunlightLevel);
            lastSunlightLevelUpdate = sunlightLevel;
        }
    }

    /// <summary>
    /// Calculates the sunlight brightness level based on the time of day.
    /// </summary>
    /// <param name="currentHour">The current time in hours (including fractional minutes).</param>
    /// <returns>The calculated sunlight brightness level.</returns>
    private float CalculateSunlightLevel(float currentHour)
    {
        // During full daylight.
        if (currentHour >= dawnEndHour && currentHour < duskStartHour)
        {
            return maxSunlightLevel;
        }
        // During full night.
        else if (currentHour >= duskEndHour || currentHour < dawnStartHour)
        {
            return minSunlightLevel;
        }
        else
        {
            // During transitional periods: dawn or dusk.
            if (currentHour >= dawnStartHour && currentHour < dawnEndHour)
            {
                float progress = (currentHour - dawnStartHour) / (float)(dawnEndHour - dawnStartHour);
                return Mathf.Lerp(minSunlightLevel, maxSunlightLevel, progress);
            }
            else if (currentHour >= duskStartHour && currentHour < duskEndHour)
            {
                float progress = (currentHour - duskStartHour) / (float)(duskEndHour - duskStartHour);
                return Mathf.Lerp(maxSunlightLevel, minSunlightLevel, progress);
            }
            else
            {
                return minSunlightLevel;
            }
        }
    }

    #endregion

    #region Public Utility Methods

    /// <summary>
    /// Returns the current time in hours (including fractional minutes).
    /// </summary>
    public float GetCurrentTime() => currentHour + currentMinute / 60f;

    /// <summary>
    /// Returns the current hour of the day.
    /// </summary>
    public int GetCurrentHour() => currentHour;

    /// <summary>
    /// Returns the current day count.
    /// </summary>
    public int GetCurrentDay() => currentDay;

    /// <summary>
    /// Returns the length of the daytime in hours.
    /// </summary>
    public int GetDayTimeLengthInHour() => nightStartHour - dayStartHour;

    /// <summary>
    /// Returns whether it is currently daytime.
    /// </summary>
    public bool IsDayTime() => isDay;

    /// <summary>
    /// Returns the percentage of the night that has passed.
    /// </summary>
    public float GetHowMuchPercentageOfNightTimeHasPassed()
    {
        if (isDay)
            return 0f;
        int nightMinutesPassed = GetNightTimePassedForToday();
        int totalNightMinutes = 24 - GetDayTimeLengthInHour();
        return (float)nightMinutesPassed / totalNightMinutes;
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Returns the number of hours passed since the start of the night.
    /// </summary>
    private int GetNightTimePassedForToday()
    {
        if (currentHour >= nightStartHour)
            return currentHour - nightStartHour;
        else
            return 24 - nightStartHour + currentHour;
    }

    #endregion
}