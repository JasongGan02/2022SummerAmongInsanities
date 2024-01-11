using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static UnityEditor.Progress;
using Image = UnityEngine.UI.Image;
using Text = UnityEngine.UI.Text;
using TMPro;
using System;

public class CraftingQueueManager : MonoBehaviour
{
    
    private Queue<BaseObject> craftQueue = new Queue<BaseObject>();
    public RectTransform content;
    public GameObject buttonPrefab;
    private float xOffset = -180f;
    private TextMeshProUGUI ProgressText;
    private GameObject QueueUI;

    private CoreArchitecture coreArchitecture;
    private TimeSystemManager timeSystemManager;

    public int sizeCraftQueue()
    {
        return craftQueue.Count;
    }
   

    void Start()
    {
        StartCoroutine(WaitForCoreArchitectureAndInitialize());
        QueueUI = GameObject.Find("QueueUI");
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
        ProgressText = QueueUI.transform.Find("TimeCount").GetComponent<TextMeshProUGUI>();
        timeSystemManager = FindObjectOfType<TimeSystemManager>();

    }


    private void UpdateQueueUI(Queue<BaseObject> craftQueue)
    {
        // Clear existing UI elements (assuming content is a transform containing old UI elements)
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Set xOffset to its initial position
        xOffset = 0;

        // Loop through each item in the craftQueue
        foreach (BaseObject item in craftQueue)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, content);
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();

            buttonRect.sizeDelta = new Vector2(100, 100);

            // Set anchored position based on the current xOffset
            buttonRect.anchoredPosition = new Vector2(xOffset, 0);

            // Increment xOffset for next button
            xOffset += (buttonRect.sizeDelta.x + 10);

            GameObject buttonImage = new GameObject("ButtonImage");
            buttonImage.transform.SetParent(buttonObj.transform);

            Image image = buttonImage.AddComponent<Image>();
            RectTransform ImageRect = buttonImage.GetComponent<RectTransform>();
            ImageRect.sizeDelta = new Vector2(80, 80); // Making the image slightly smaller than the button
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

            float remainingCraftingTime = (itemToCraft as ICraftableObject).getCraftTime();

            Action<int> hourUpdateHandler = null;
            hourUpdateHandler = (hour) =>
            {
                remainingCraftingTime -= 1; // adjust this value as needed
                ProgressText.text = remainingCraftingTime.ToString() + "'s";

                if (remainingCraftingTime == 0)
                {
                    // Item crafting is complete; you can handle item creation here
                    if (coreArchitecture != null)
                    spawn(itemToCraft);

                    // Remove the item from the queue
                    craftQueue.Dequeue();
                    UpdateQueueUI(craftQueue);

                    // IMPORTANT: Unsubscribe from the event to prevent multiple calls
                    timeSystemManager.OnHourUpdatedHandler -= hourUpdateHandler;
                }
            };

            timeSystemManager.OnHourUpdatedHandler += hourUpdateHandler;

            // Wait for crafting time to complete (Note: This is just a safety wait, adjust the value as required)
            yield return new WaitForSeconds(remainingCraftingTime * timeSystemManager.dayToRealTimeInSecond / 24);
        }
    }



    private void spawn(BaseObject itemToCraft)
    {
        
            GameObject spawnObject = Instantiate(itemToCraft.getPrefab(), coreArchitecture.transform.position + new Vector3(1, 0, 0), Quaternion.Euler(new Vector3(0, 0, 90)));
            spawnObject.layer = LayerMask.NameToLayer("resource");
            var controller = spawnObject.AddComponent<DroppedObjectController>();
            controller.Initialize(itemToCraft as IInventoryObject, 1);
        
            
    }

    IEnumerator WaitForCoreArchitectureAndInitialize()
    {
        while (CoreArchitecture.Instance == null)
        {
            yield return null;
        }

        coreArchitecture = CoreArchitecture.Instance;
    }


}
