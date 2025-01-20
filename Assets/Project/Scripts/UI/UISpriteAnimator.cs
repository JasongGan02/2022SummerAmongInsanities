using UnityEngine;
using UnityEngine.UI;
using System;

public class UISpriteAnimator : MonoBehaviour
{
    public Image targetImage; // The UI Image to animate
    public Sprite[] animationSprites; // Array of sprites to animate
    public float frameRate = 10f; // Frames per second
    public bool loop = true; // Whether the animation should loop
    public bool playOnAwake = true; // Play the animation automatically on start

    private bool isPlaying = false; // Whether the animation is currently playing
    private bool isPaused = false; // Whether the animation is paused
    private float timer = 0f; // Timer to track frame changes
    private int currentFrame = 0; // Current frame index
    private float playbackSpeed = 1f; // Speed multiplier for animation playback

    public event Action OnAnimationComplete; // Event triggered when animation finishes

    private void Start()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (playOnAwake)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!isPlaying || isPaused || animationSprites.Length == 0) return;

        timer += Time.deltaTime * playbackSpeed;

        if (timer >= 1f / frameRate)
        {
            timer = 0f;
            currentFrame++;

            if (currentFrame >= animationSprites.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = animationSprites.Length - 1; // Stay on the last frame
                    Stop();
                    OnAnimationComplete?.Invoke();
                    return;
                }
            }

            targetImage.sprite = animationSprites[currentFrame];
        }
    }

    // Public Methods for Animation Control

    /// <summary>
    /// Starts the animation from the first frame.
    /// </summary>
    public void Play()
    {
        isPlaying = true;
        isPaused = false;
        currentFrame = 0;
        timer = 0f;

        if (animationSprites.Length > 0)
        {
            targetImage.sprite = animationSprites[currentFrame];
        }
    }

    /// <summary>
    /// Stops the animation and resets to the first frame.
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
        isPaused = false;
        currentFrame = 0;

        if (animationSprites.Length > 0)
        {
            targetImage.sprite = animationSprites[0]; // Reset to the first frame
        }
    }

    /// <summary>
    /// Pauses the animation at the current frame.
    /// </summary>
    public void Pause()
    {
        isPaused = true;
    }

    /// <summary>
    /// Resumes the animation from the current frame.
    /// </summary>
    public void Resume()
    {
        isPaused = false;
    }

    /// <summary>
    /// Changes the playback speed of the animation.
    /// </summary>
    /// <param name="speed">Speed multiplier (1.0 = normal speed).</param>
    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = Mathf.Max(0.1f, speed); // Ensure the speed is not zero or negative
    }

    /// <summary>
    /// Jumps to a specific frame in the animation.
    /// </summary>
    /// <param name="frameIndex">The frame index to jump to (0-based).</param>
    public void JumpToFrame(int frameIndex)
    {
        if (frameIndex >= 0 && frameIndex < animationSprites.Length)
        {
            currentFrame = frameIndex;
            targetImage.sprite = animationSprites[currentFrame];
        }
        else
        {
            Debug.LogWarning("Frame index out of bounds.");
        }
    }

    /// <summary>
    /// Checks if the animation is currently playing.
    /// </summary>
    /// <returns>True if the animation is playing, false otherwise.</returns>
    public bool IsPlaying()
    {
        return isPlaying && !isPaused;
    }

    /// <summary>
    /// Gets the current frame index.
    /// </summary>
    /// <returns>The current frame index (0-based).</returns>
    public int GetCurrentFrame()
    {
        return currentFrame;
    }
}
