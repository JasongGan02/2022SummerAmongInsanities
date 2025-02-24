using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Image = UnityEngine.UI.Image;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI; // Import for Slider

public class CraftingQueueManager : MonoBehaviour
{
    private Queue<BaseObject> craftQueue = new Queue<BaseObject>();
    public RectTransform content;
    public GameObject buttonPrefab;
    private float xOffset = -180f;
    private TextMeshProUGUI progressText;
    public GameObject queueUI;

    private CoreArchitectureController coreArchitecture;
    private TimeSystemManager timeSystemManager;

    public Slider craftingProgressSlider; // Reference to the Slider
    private float currentCraftTime = 0f; // Track the time passed during crafting
    private float totalCraftTime = 0f; // Total time to craft the current item

    public int sizeCraftQueue()
    {
        return craftQueue.Count;
    }

    void Start()
    {
        StartCoroutine(WaitForCoreArchitectureAndInitialize());
        progressText = queueUI.transform.Find("TimeCount").GetComponent<TextMeshProUGUI>();
        timeSystemManager = TimeSystemManager.Instance;
        GameObject sliderObject = GameObject.Find("CraftingProgressSlider");
        if (sliderObject != null)
        {
            craftingProgressSlider = sliderObject.GetComponent<Slider>();
        }

        // Make sure the slider starts at 0
        if (craftingProgressSlider != null)
        {
            craftingProgressSlider.value = 1f;
            craftingProgressSlider.gameObject.SetActive(false); // Hide the slider when no crafting
        }
    }

    private void UpdateQueueUI(Queue<BaseObject> craftQueue)
    {
        // Clear existing UI elements
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        xOffset = 0;

        // Loop through each item in the craftQueue
        foreach (BaseObject item in craftQueue)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, content);
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();

            buttonRect.sizeDelta = new Vector2(106, 106);
            buttonRect.anchoredPosition = new Vector2(xOffset, 0);

            xOffset += (buttonRect.sizeDelta.x + 10);

            GameObject buttonImage = new GameObject("ButtonImage");
            buttonImage.transform.SetParent(buttonObj.transform);

            Image image = buttonImage.AddComponent<Image>();
            RectTransform ImageRect = buttonImage.GetComponent<RectTransform>();
            ImageRect.sizeDelta = new Vector2(92, 92);
            ImageRect.localPosition = new Vector2(0, 0);
            image.sprite = item.getPrefabSprite();
        }
    }

    // Add an item to the crafting queue
    public void AddToQueue(BaseObject outputItem, Action onQueueComplete = null)
    {
        craftQueue.Enqueue(outputItem);
        UpdateQueueUI(craftQueue);

        if (craftQueue.Count == 1)
        {
            StartCoroutine(CraftItemFromQueue(onQueueComplete));
        }
    }

    // Coroutine to craft the first item in the queue
    private IEnumerator CraftItemFromQueue(Action onQueueComplete)
    {
        while (craftQueue.Count > 0)
        {
            BaseObject itemToCraft = craftQueue.Peek();
            totalCraftTime = (itemToCraft as ICraftableObject).getCraftTime();
            float remainingCraftingTime = totalCraftTime;

            // Show the slider when crafting starts
            if (craftingProgressSlider != null)
            {
                craftingProgressSlider.gameObject.SetActive(true); // Show the slider
                craftingProgressSlider.value = 1f; // Set it to full (1)
            }

            // Update crafting progress based on elapsed real time
            while (remainingCraftingTime > 0)
            {
                yield return null; // Wait for the next frame

                // Reduce remaining crafting time by the time elapsed since the last frame
                float deltaTime = Time.deltaTime;
                remainingCraftingTime -= deltaTime;

                // Update slider and progress text
                if (craftingProgressSlider != null)
                {
                    float progress = Mathf.Clamp01(remainingCraftingTime / totalCraftTime);
                    craftingProgressSlider.value = progress;
                }

                if (progressText != null)
                {
                    progressText.text = Mathf.CeilToInt(remainingCraftingTime).ToString() + "s"; // Update text
                }
            }

            craftQueue.Dequeue();
            UpdateQueueUI(craftQueue);
            onQueueComplete?.Invoke();
        }

        // Hide the slider when crafting is finished
        if (craftingProgressSlider != null)
        {
            craftingProgressSlider.gameObject.SetActive(false);
        }
    }

    IEnumerator WaitForCoreArchitectureAndInitialize()
    {
        while (CoreArchitectureController.Instance == null)
        {
            yield return null;
        }

        coreArchitecture = CoreArchitectureController.Instance;
    }
}