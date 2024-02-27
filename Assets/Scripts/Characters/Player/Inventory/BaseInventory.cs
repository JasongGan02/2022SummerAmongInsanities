using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseInventory : MonoBehaviour, BaseInventory.InventoryButtonClickedCallback
{
    public GameObject defaultRow;
    public GameObject template;
    public GameObject background;

    public int defaultNumberOfRow = 6;

    protected InventoryEventBus eventBus;
    protected InventoryUiController uiController;
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

    public void AddItem(IInventoryObject item, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            int indexToUpdate = database.AddItem(item);
            UpdateSlotUi(indexToUpdate);
        }
    }

    

    public void UpdateSlotUi(int index)
    {
        uiController.UpdateSlotUi(index, database.GetInventorySlotAtIndex(index));
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

    public interface InventoryButtonClickedCallback
    {
        void Sort();
    }



}
