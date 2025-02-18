using UnityEngine;
using UnityEngine.UI;

public class DayNightController : MonoBehaviour
{
    [Header("Day, Night, and Blood Moon Objects")]
    [SerializeField] private GameObject dayObject;     // Day object
    [SerializeField] private GameObject nightObject;   // Night object
    [SerializeField] private GameObject bloodMoonObject;  // Blood Moon object

    [Header("Sprites for Day, Night, and Blood Moon Animations")]
    [SerializeField] private Sprite[] dayFrames;      // Sprites for day animation
    [SerializeField] private Sprite[] nightFrames;    // Sprites for night animation
    [SerializeField] private Sprite[] bloodMoonFrames; // Sprites for blood moon animation

    [Header("Animation Speed Control")]
    [SerializeField, Range(0.1f, 2f)] private float animationSpeed = 1.0f;  // Control the play speed

    private Image currentImage;
    private int currentFrame = 0;
    private bool isDay;
    private float frameTime = 0f;
    private Sprite[] currentAnimationFrames;
    private GameObject activeObject;

    private void Start()
    {
        if (dayObject == null || nightObject == null || bloodMoonObject == null)
        {
            Debug.LogError("Day, Night, and Blood Moon objects must be assigned.");
            return;
        }

        // Initialize objects and clock state
        InitializeClockState();

        // Subscribe to events
        GameEvents.current.OnDayStarted += OnDayStarted;
        GameEvents.current.OnNightStarted += OnNightStarted;
    }

    private void InitializeClockState()
    {
        // Check if it's day or night, and initialize accordingly
        if (TimeSystemManager.Instance.IsDayTime())
        {
            OnDayStarted();
        }
        else
        {
            bool isBloodMoon = TimeSystemManager.Instance.GetCurrentDay() % TimeSystemManager.Instance.redMoonNightInterval == 0;
            OnNightStarted(isBloodMoon);
        }
    }

    private void Update()
    {
        // Only update frames if there is an active animation
        if (currentAnimationFrames != null)
        {
            frameTime += Time.deltaTime * animationSpeed;

            // Get the current frame based on frameTime
            int frameIndex = Mathf.FloorToInt(frameTime);
            
            if (frameIndex < currentAnimationFrames.Length)
            {
                // If we're within the first half of the period (0-5), hold on frame 5 after the first transition
                if (frameIndex <= 5)
                {
                    currentImage.sprite = currentAnimationFrames[frameIndex];
                }
                else if (frameIndex >= 6 && frameIndex <= 11)
                {
                    // If we're within the second half of the period (6-11), play the second part of the animation
                    currentImage.sprite = currentAnimationFrames[frameIndex];
                }
            }

            // Hold the frame at index 5 for the majority of the period
            if (frameIndex >= 5 && frameIndex < 6)
            {
                currentImage.sprite = currentAnimationFrames[5];
            }

            // When the animation reaches the last frame, reset frame time and move to the next set of frames
            if (frameIndex >= 11)
            {
                frameTime = 0f;
                currentFrame = 0;
                SwitchToNextObject();
            }
        }
    }

    private void OnDayStarted()
    {
        isDay = true;
        activeObject = dayObject;

        dayObject.SetActive(true);
        nightObject.SetActive(false);
        bloodMoonObject.SetActive(false);

        // Set the current animation frames for day and reset frame time
        currentAnimationFrames = dayFrames;
        frameTime = 0f;

        // Get the Image component of the current active object
        currentImage = dayObject.GetComponent<Image>();
    }

    private void OnNightStarted(bool isBloodMoon)
    {
        isDay = false;
        activeObject = isBloodMoon ? bloodMoonObject : nightObject;

        dayObject.SetActive(false);
        nightObject.SetActive(!isBloodMoon);
        bloodMoonObject.SetActive(isBloodMoon);

        // Set the current animation frames based on whether it's a blood moon or night
        currentAnimationFrames = isBloodMoon ? bloodMoonFrames : nightFrames;
        frameTime = 0f;

        // Get the Image component of the current active object
        currentImage = activeObject.GetComponent<Image>();
    }

    private void SwitchToNextObject()
    {
        // Switch between day, night, and blood moon objects based on the time of day
        if (isDay)
        {
            // Transition from day to night
            OnNightStarted(TimeSystemManager.Instance.GetCurrentDay() % TimeSystemManager.Instance.redMoonNightInterval == 0);
        }
        else
        {
            // Transition from night or blood moon back to day
            OnDayStarted();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        GameEvents.current.OnDayStarted -= OnDayStarted;
        GameEvents.current.OnNightStarted -= OnNightStarted;
    }
}