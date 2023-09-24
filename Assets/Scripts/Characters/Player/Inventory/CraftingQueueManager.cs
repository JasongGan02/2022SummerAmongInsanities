using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static UnityEditor.Progress;
using Image = UnityEngine.UI.Image;
using Text = UnityEngine.UI.Text;
using TMPro;

public class CraftingQueueManager : MonoBehaviour
{
    
    private Queue<BaseObject> craftQueue = new Queue<BaseObject>();
    public RectTransform content;
    public GameObject buttonPrefab;
    private float xOffset = -180f;
    private TextMeshProUGUI ProgressText;
    private GameObject QueueUI;

    private CoreArchitecture coreArchitecture;
 


    void Start()
    {
        QueueUI = GameObject.Find("QueueUI");
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
        ProgressText = QueueUI.transform.Find("TimeCount").GetComponent<TextMeshProUGUI>();
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

            while (remainingCraftingTime > 0)
            {
                yield return new WaitForSeconds(1.0f);
                remainingCraftingTime -= 1.0f;
                ProgressText.text = remainingCraftingTime.ToString()+"'s";
            }

            // Item crafting is complete; you can handle item creation here

            spawn(itemToCraft);

            // Remove the item from the queue
            craftQueue.Dequeue();
            UpdateQueueUI(craftQueue);
        }
    }


    private void spawn(BaseObject itemToCraft)
    {
        GameObject spawnObject = Instantiate(itemToCraft.getPrefab(), coreArchitecture.transform.position + new Vector3(1, 0, 0), Quaternion.Euler(new Vector3(0, 0, 90)));
        spawnObject.layer = LayerMask.NameToLayer("resource");
        var controller = spawnObject.AddComponent<DroppedObjectController>();
        controller.Initialize(itemToCraft as IInventoryObject, 1);
    }




}
