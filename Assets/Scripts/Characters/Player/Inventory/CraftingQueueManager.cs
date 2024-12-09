using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using static UnityEditor.Progress;
using Image = UnityEngine.UI.Image;
using Text = UnityEngine.UI.Text;
using TMPro;
using UnityEngine.UI;  // Import for Slider

public class CraftingQueueManager : MonoBehaviour
{
    private Queue<BaseObject> craftQueue = new Queue<BaseObject>();
    public RectTransform content;
    public GameObject buttonPrefab;
    private float xOffset = -180f;
    private TextMeshProUGUI ProgressText;
    private GameObject QueueUI;

    private CoreArchitectureController coreArchitecture;
    private TimeSystemManager timeSystemManager;

    public Slider craftingProgressSlider; // Reference to the Slider
    private float currentCraftTime = 0f; // Track the time passed during crafting
    private float totalCraftTime = 0f;   // Total time to craft the current item

    public int sizeCraftQueue()
    {
        return craftQueue.Count;
    }

    void Start()
    {
        StartCoroutine(WaitForCoreArchitectureAndInitialize());
        QueueUI = GameObject.Find("QueueUI");
        coreArchitecture = FindObjectOfType<CoreArchitectureController>();
        ProgressText = QueueUI.transform.Find("TimeCount").GetComponent<TextMeshProUGUI>();
        timeSystemManager = FindObjectOfType<TimeSystemManager>();
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
    public void AddToQueue(BaseObject outputitem)
    {
        craftQueue.Enqueue(outputitem);
        UpdateQueueUI(craftQueue);

        if (craftQueue.Count == 1)
        {
            StartCoroutine(CraftItemFromQueue());
        }
    }

    // Coroutine to craft the first item in the queue
    private IEnumerator CraftItemFromQueue()
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

            Action<int> hourUpdateHandler = null;
            hourUpdateHandler = (hour) =>
            {
                remainingCraftingTime -= 1; // Decrease crafting time
                float progress = remainingCraftingTime / totalCraftTime;
                craftingProgressSlider.value = progress; // Update slider

                ProgressText.text = remainingCraftingTime.ToString() + "'s"; // Update text

                if (remainingCraftingTime <= 0)
                {
                    // Crafting complete
                    if (coreArchitecture != null)
                        spawn(itemToCraft);

                    craftQueue.Dequeue();
                    UpdateQueueUI(craftQueue);

                    // Unsubscribe from the event to prevent multiple calls
                    GameEvents.current.OnHourUpdated -= hourUpdateHandler;
                }
            };

            GameEvents.current.OnHourUpdated += hourUpdateHandler;

            // Wait for crafting to complete
            yield return new WaitForSeconds(remainingCraftingTime * timeSystemManager.dayToRealTimeInSecond / 24);
        }
    }

    private void spawn(BaseObject itemToCraft)
    {
        GameObject drop = ((ICraftableObject)itemToCraft).GetDroppedGameObject(1, CoreArchitectureController.Instance.transform.position);
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
