using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Lumin;
using UnityEngine.Experimental.Rendering;

public class Inventory : BaseInventory, Inventory.InventoryButtonClickedCallback
{

    public GameObject extraRow;
    public int maxExtraRow = 4;

    private GameObject player;
    private GameObject hotbar;
    private CraftingQueueManager queueManager;
    


    protected override void Awake()
    {
        inventoryGrid = GameObject.Find(Constants.Name.INVENTORY_GRID);
        database = new InventoryDatabase(defaultNumberOfRow, maxExtraRow);
        uiController = new InventoryUiController(
            defaultNumberOfRow,
            defaultRow,
            extraRow,
            inventoryGrid,
            template,
            this,
            FindObjectOfType<UIViewStateManager>()
            );

        uiController.SetupUi();
        hotbar = inventoryGrid.transform.GetChild(0).gameObject;

        eventBus = new InventoryEventBus();
        queueManager = FindObjectOfType<CraftingQueueManager>();
    }

    void Update()
    {
        if(player==null) player = GameObject.FindGameObjectWithTag("Player");
        HandleHotbarKeyPress();
    }

   

    // Inventory operation
    public InventorySlot GetInventorySlotAtIndex(int index)
    {
        return database.GetInventorySlotAtIndex(index);
    }


    public void RemoveItemByOne(int index)
    {
        database.RemoveItemByOne(index);
        UpdateSlotUi(index);
    }

    public void RemoveItemAndDrop(int index)
    {
        InventorySlot removedItem = database.RemoveItem(index);
        if (removedItem != null)
        {
            Vector3 dropPosition;
            if (player.GetComponent<Playermovement>().facingRight)
            {
                dropPosition = player.transform.position + new Vector3(1, 0, 0);
            }
            else
            {
                dropPosition = player.transform.position + new Vector3(-1, 0, 0);
            }
            // TODO refactor collectible object to set the amount when getting the dropped item
            GameObject droppedItem = removedItem.item.GetDroppedGameObject(removedItem.count);
            droppedItem.transform.position = dropPosition;
            droppedItem.GetComponent<DroppedObjectController>().Initialize(removedItem.item, removedItem.count);
        }

        UpdateSlotUi(index);
    }

    public void RemoveAllItemsAndDrops()
    {
        for (int i = 0; i < database.GetSize(); i++)
        {
            InventorySlot removedItem = database.RemoveItem(i);
            if (removedItem != null)
            {
                Vector3 dropPosition;
                if (player.GetComponent<Playermovement>().facingRight)
                {
                    dropPosition = player.transform.position + new Vector3(1, 0, 0);
                }
                else
                {
                    dropPosition = player.transform.position + new Vector3(-1, 0, 0);
                }
                // TODO refactor collectible object to set the amount when getting the dropped item
                GameObject droppedItem = removedItem.item.GetDroppedGameObject(removedItem.count);
                droppedItem.transform.position = dropPosition;
                droppedItem.GetComponent<DroppedObjectController>().Initialize(removedItem.item, removedItem.count);
            }

            UpdateSlotUi(i);
        }
    }

    public void SwapItems(int index1, int index2)
    {
        database.SwapItems(index1, index2);

        UpdateSlotUi(index1);
        UpdateSlotUi(index2);
    }

    public int MoveItems(int fromIndex, int toIndex, int amount, bool shouldUpdateFromSlot)
    {
        int remainingItemCount = database.MoveItems(fromIndex, toIndex, amount);
        if (remainingItemCount >= 0)
        {
            if (shouldUpdateFromSlot)
            {
                UpdateSlotUi(fromIndex);
            }
            
            UpdateSlotUi(toIndex);
        }
        return remainingItemCount;
    }



    public void CraftItems(BaseObject[] items, int[] quantity, BaseObject outputItem)
    {
        if (queueManager.sizeCraftQueue() >= 4)
        {
            return;
        }

        // First, check if all items are available in required quantities
        for (int i = 0; i < items.Length; i++)
        {
            
            if ((HasEnoughItem(items[i] as IInventoryObject, quantity[i]) == false))
            {
                
                // If any item is not available in required quantity, exit the method
                return;
            }
        }

        // Since all items are available, consume them
        for (int i = 0; i < items.Length; i++)
        {
            ConsumeItem(items[i] as IInventoryObject, quantity[i]);
        }

        // Add the output item to the crafting queue
        queueManager.AddToQueue(outputItem);
    }


    private bool HasEnoughItem(IInventoryObject inventoryObject, int requiredQuantity)
    {
        bool foundItem = false;

        for (int i = 0; i < database.GetSize(); i++)
        {
            InventorySlot slot = database.GetInventorySlotAtIndex(i);

            if (!slot.IsEmpty && (slot.item as BaseObject) == (inventoryObject as BaseObject))
            {
                foundItem = true;

                if (requiredQuantity > slot.count)
                {
                    Debug.Log("Not Enough: " + slot.item.GetItemName());
                    return false;
                }
            }
        }

        return foundItem;
    }



    // TODO: check if there's an existing slot that is not full
    public bool CanAddItem(IInventoryObject item)
    {
        return database.HasEmptySlot();
    }

    

    private void HandleHotbarKeyPress() 
    {
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseItemInHotbarSlot(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UseItemInHotbarSlot(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            UseItemInHotbarSlot(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            UseItemInHotbarSlot(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            UseItemInHotbarSlot(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            UseItemInHotbarSlot(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            UseItemInHotbarSlot(6);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            UseItemInHotbarSlot(7);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            UseItemInHotbarSlot(8);
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            UseItemInHotbarSlot(9);
        }
    }

    private void UseItemInHotbarSlot(int index)
    {
        if (index > 9) return;

        GameObject slot = hotbar.transform.GetChild(0).GetChild(index).gameObject;
        if (slot.transform.childCount > 0)
        {
            eventBus.OnSlotLeftClicked(index);
        }
    }

    

    public void Upgrade()
    {
        if (database.CanUpgrade())
        {
            database.Upgrade();
            uiController.Upgrade();
        }
    }

    //Find number of exp in inventory and return it for Inventory Upgrade and Rogue Level Up
    public int checkEXP()
    {
        for (int i = 0; i < database.GetSize(); i++)
        {
            InventorySlot slot = database.GetInventorySlotAtIndex(i);
            if (slot != null && slot.item is DivinityFragObject)
            {
                return slot.count;
            }
        }
        return 0;
    }

    public bool spendEXP(int cost)
    {   
        
        for (int i = 0; i < database.GetSize(); i++)
        {
            InventorySlot slot = database.GetInventorySlotAtIndex(i);
   
            if (!slot.IsEmpty && slot.item is DivinityFragObject)
            {
                
                if (cost > slot.count)
                {
                    Debug.Log("Not Enough Divinity Fragments");
                    return false;
                }
                else 
                {
                    database.RemoveItemByOne(i, cost);
                    UpdateSlotUi(i);
                    return true;
                }
                    
            }
        }
        
        return false;
    }

    public bool ConsumeItem(IInventoryObject inventoryObject, int num)
    {
        for (int i = 0; i < database.GetSize(); i++)
        {
            InventorySlot slot = database.GetInventorySlotAtIndex(i);
            if (!slot.IsEmpty && (slot.item as BaseObject).itemName == (inventoryObject as BaseObject).itemName)
            {
                if (num > slot.count)
                {
                    Debug.Log("Not Enough: " + slot.item.GetItemName());
                    return false;
                }
                else
                {
                    database.RemoveItemByOne(i, num);
                    UpdateSlotUi(i);
                    return true;
                }

            }
        }
        return false;
    }


    public InventorySlot findSLOT (IInventoryObject inventoryObject)
    {   
        
        for (int i = 0; i < database.GetSize(); i++)
        {
            InventorySlot slot = database.GetInventorySlotAtIndex(i);
    
            if (slot != null && (slot.item as BaseObject).itemName == (inventoryObject as BaseObject).itemName)
            {
                
                return slot;
                    
            }
        }

        Debug.Log("Item not found in the inventory");
        return null;
    }

    public int findSLOTINDEX(IInventoryObject inventoryObject)
    {

        for (int i = 0; i < database.GetSize(); i++)
        {
            InventorySlot slot = database.GetInventorySlotAtIndex(i);

            if (slot != null && (slot.item as BaseObject).itemName == (inventoryObject as BaseObject).itemName)
            {

                return i;

            }
        }

        return -1;
    }
    public int findSlotIndex(string inventoryObject)
    {

        for (int i = 0; i < database.GetSize(); i++)
        {
            InventorySlot slot = database.GetInventorySlotAtIndex(i);

            if (slot != null && (slot.item as BaseObject).itemName == inventoryObject)
            {

                return i;

            }
        }

        return -1;
    }


    public InventorySlot findSlot(string inventoryObject)
    {
       
        for (int i = 0; i < database.GetSize(); i++)
        {
            InventorySlot slot = database.GetInventorySlotAtIndex(i);

            if (slot != null && (slot.item as BaseObject).itemName == inventoryObject)
            {

                return slot;

            }
        }

        return null;
    }


    public int findItemCount(BaseObject inventoryObject)
    {
        // Ensure that inventoryObject is not null
        if (inventoryObject == null)
        {
            Debug.LogError("inventoryObject is null");
            return 0;
        }

        int count = 0;

        // Ensure that database is not null
        if (database != null)
        {
            for (int i = 0; i < database.GetSize(); i++)
            {
                InventorySlot slot = database.GetInventorySlotAtIndex(i);

                // Ensure that slot and slot.item are not null
                if (slot != null && slot.item != null)
                {
                    BaseObject baseObject = slot.item as BaseObject;
                    if (baseObject != null && baseObject.itemName == inventoryObject.itemName)
                    {
                        count += slot.count;
                    }
                }
            }
        }
        else
        {
            Debug.LogError("database is null");
        }

        return count;
    }



    public void DropAxe()
    {
        //Instantiate(axe);
    }


    public new interface InventoryButtonClickedCallback
    {
        void Sort();
        void Upgrade();
        void DropAxe();
    }
}
