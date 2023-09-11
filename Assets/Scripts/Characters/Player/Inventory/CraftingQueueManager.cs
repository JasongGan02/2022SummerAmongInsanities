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


    private CoreArchitecture coreArchitecture;
 


    void Start()
    {
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
        ProgressText = GameObject.Find("TimeCount").GetComponent<TextMeshProUGUI>();
    }
    

    private void UpdateQueueUI(BaseObject item)
    {

            GameObject buttonObj = Instantiate(buttonPrefab, content);
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();

            buttonRect.sizeDelta = new Vector2(100,100);
            buttonRect.anchoredPosition = new Vector2(xOffset, 0);
            xOffset += (buttonRect.sizeDelta.x+10);
            

            GameObject buttonImage = new GameObject();
            buttonImage.transform.SetParent(buttonObj.transform);

            Image image = buttonImage.AddComponent<Image>();
            RectTransform ImageRect = buttonImage.GetComponent<RectTransform>();
            ImageRect.localPosition = new Vector2(0, 0);
            image.sprite = item.getPrefabSprite();
    }

    private void DestroyQueueUI()
    {

        Destroy(GameObject.Find("ButtonPrefab(Clone)"));
    }


    // Add an item to the crafting queue
    public void AddToQueue(BaseObject outputitem)
    {
        craftQueue.Enqueue(outputitem);
        UpdateQueueUI(outputitem);
         
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
                ProgressText.text = remainingCraftingTime.ToString();
            }

            // Item crafting is complete; you can handle item creation here

            spawn(itemToCraft);

            // Remove the item from the queue
            craftQueue.Dequeue();
            DestroyQueueUI();
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
