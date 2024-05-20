using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseInventory : MonoBehaviour, BaseInventory.IInventoryButtonClickedCallback
{
    public GameObject defaultRow;
    public GameObject template;
    public GameObject background;
    protected GameObject player;
    public int defaultNumberOfRow = 2;

    protected InventoryEventBus eventBus;
    public InventoryUiController uiController;
    protected GameObject inventoryGrid;
    protected InventoryDatabase database;

    protected virtual void Awake()
    {
        inventoryGrid = GameObject.Find(Constants.Name.CHESTINVENTORY_GRID);
        database = new InventoryDatabase(defaultNumberOfRow, 0);
        uiController = new InventoryUiController(
            defaultNumberOfRow,
            defaultRow,
            inventoryGrid,
            template,
            this,
            FindObjectOfType<UIViewStateManager>()
            );
        uiController.SetupChestUi();
    }

    protected virtual void Update()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
    }

    public void SetCurrentInventory(InventoryDatabase newDatabase)
    {
        if (this.database != newDatabase)
        {
            this.database = newDatabase; // Update the internal database reference
            UpdateWholeInventory();
        }
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
            GameObject droppedItem = removedItem.item.GetDroppedGameObject(removedItem.count, dropPosition);
            droppedItem.GetComponent<DroppedObjectController>().Initialize(removedItem.item, removedItem.count);
        }

        UpdateSlotUi(index);
    }



    // Inventory operation
    public InventorySlot GetInventorySlotAtIndex(int index)
    {
        return database.GetInventorySlotAtIndex(index);
    }

    public void SwapItems(int index1, int index2)
    {
        database.SwapItems(index1, index2);

        UpdateSlotUi(index1);
        UpdateSlotUi(index2);
    }

    public void SwapItemsBetweenInventory(BaseInventory targetInventory,int index1, int index2)
    {
        this.database.TransferItemToOtherInventory(targetInventory.database,index1, index2);
        UpdateSlotUi(index1);
        targetInventory.UpdateSlotUi(index2);
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


    public void UpdateSlotUi(int index)
    {
        uiController.UpdateSlotUi(index, database.GetInventorySlotAtIndex(index));
    }

    public void Sort()
    {
        this.database.Sort();
        UpdateWholeInventory();

    }

    private void UpdateWholeInventory()
    {
        int size = this.database.GetSize();
        for (int i = 0; i < size; i++)
        {
            UpdateSlotUi(i);
        }
    }

    public interface IInventoryButtonClickedCallback
    {
        void Sort();
    }



}
