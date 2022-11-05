using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour, Inventory.InventoryButtonClickedCallback
{
    public GameObject defaultRow;
    public GameObject extraRow;
    public GameObject template;
    public GameObject background;
    public GameObject axe;

    public int defaultNumberOfRow = 2;
    public int maxExtraRow = 4;
    
    private GameObject inventoryGrid;
    private GameObject hotbar;

    private InventoryEventBus eventBus;
    private InventoryUiController uiController;
    private InventoryDatabase database;

    void Awake()
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
        
    }

    void Update()
    {
        HandleHotbarKeyPress();
    }

    // Event system
    public void OnSlotRightClicked(int index, bool isShiftDown)
    {
        eventBus.OnSlotRightClicked(index, isShiftDown);
    }

    public void OnSlotLeftClicked(int index)
    {
        eventBus.OnSlotLeftClicked(index);
    }

    public void AddSlotRightClickedHandler(EventHandler<InventoryEventBus.OnSlotRightClickedEventArgs> action)
    {
        if (eventBus != null)
        {
            eventBus.OnSlotRightClickedEvent += action;
        }
    }

    public void RemoveSlotRightClickedHandler(EventHandler<InventoryEventBus.OnSlotRightClickedEventArgs> action)
    {
        if (eventBus != null)
        {
            eventBus.OnSlotRightClickedEvent -= action;
        }
    }

    public void AddSlotLeftClickedHandler(EventHandler<InventoryEventBus.OnSlotLeftClickedEventArgs> action)
    {
        if (eventBus != null)
        {
            eventBus.OnSlotLeftClickedEvent += action;
        }
    }

    public void RemoveSlotLeftClickedHandler(EventHandler<InventoryEventBus.OnSlotLeftClickedEventArgs> action)
    {
        if (eventBus != null)
        {
            eventBus.OnSlotLeftClickedEvent -= action;
        }
    }

    // Inventory operation
    public InventorySlot GetInventorySlotAtIndex(int index)
    {
        return database.GetInventorySlotAtIndex(index);
    }

    public void AddItem(IInventoryObject item, int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            int indexToUpdate = database.AddItem(item);
            UpdateSlotUi(indexToUpdate);
        }
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
            if (gameObject.GetComponent<Playermovement>().facingRight)
            {
                dropPosition = gameObject.transform.position + new Vector3(1, 0, 0);
            }
            else
            {
                dropPosition = gameObject.transform.position + new Vector3(-1, 0, 0);
            }
            // TODO refactor collectible object to set the amount when getting the dropped item
            GameObject droppedItem = removedItem.item.GetDroppedGameObject(removedItem.count);
            droppedItem.transform.position = dropPosition;
        }

        UpdateSlotUi(index);
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

    // TODO: check if there's an existing slot that is not full
    public bool CanAddItem(IInventoryObject item)
    {
        return database.HasEmptySlot();
    }

    private void UpdateSlotUi(int index)
    {
        uiController.UpdateSlotUi(index, database.GetInventorySlotAtIndex(index));
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

    public void Sort()
    {
        database.Sort();
        int size = database.GetSize();
        for (int i = 0; i < size; i++)
        {
            UpdateSlotUi(i);
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

    public void DropAxe()
    {
        //Instantiate(axe);
    }

    public interface InventoryButtonClickedCallback
    {
        void Sort();
        void Upgrade();
        void DropAxe();
    }
}
