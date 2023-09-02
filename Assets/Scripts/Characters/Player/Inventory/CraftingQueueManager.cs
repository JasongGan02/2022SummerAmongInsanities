using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static UnityEditor.Progress;

public class CraftingQueueManager : MonoBehaviour
{
    
    private Queue<BaseObject> craftQueue = new Queue<BaseObject>();
    private CoreArchitecture coreArchitecture;
 


    void Start()
    {
        coreArchitecture = FindObjectOfType<CoreArchitecture>();

    }

    // Add an item to the crafting queue
    public void AddToQueue(BaseObject outputitem)
    {
        craftQueue.Enqueue(outputitem);
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
            }

            // Item crafting is complete; you can handle item creation here
            GameObject spawnObject = Instantiate(itemToCraft.getPrefab(), coreArchitecture.transform.position + new Vector3(1, 0, 0), Quaternion.Euler(new Vector3(0, 0, 90)));
            spawnObject.layer = LayerMask.NameToLayer("resource");
            var controller = spawnObject.AddComponent<DroppedObjectController>();
            controller.Initialize(itemToCraft as IInventoryObject, 1);


            // Remove the item from the queue
            craftQueue.Dequeue();

            // Check if there are more items in the queue
            if (craftQueue.Count > 0)
            {
                // Craft the next item in the queue
                itemToCraft = craftQueue.Peek();
                StartCoroutine(CraftItemFromQueue());
            }
        }
    }
}
