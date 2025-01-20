using UnityEngine;
using UnityEngine.UI;

public class FrameRateController : MonoBehaviour
{
    [SerializeField]
    private Slider frameRateSlider;

    [SerializeField]
    private Text frameRateText;

    [SerializeField]
    private int minFrameRate = 30;

    [SerializeField]
    private int maxFrameRate = 120;

    private void Start()
    {
        // Initialize the slider
        frameRateSlider.minValue = minFrameRate;
        frameRateSlider.maxValue = maxFrameRate;
        frameRateSlider.value = Application.targetFrameRate;
        
        // Update the text to reflect the current frame rate
        frameRateText.text = "Frame Rate: " + Application.targetFrameRate;
        
        // Add a listener to update the frame rate when the slider is moved
        frameRateSlider.onValueChanged.AddListener(OnFrameRateChanged);
    }

    private void OnFrameRateChanged(float value)
    {
        int frameRate = Mathf.RoundToInt(value);
        Application.targetFrameRate = frameRate;
        frameRateText.text = "Frame Rate: " + frameRate;
    }
}