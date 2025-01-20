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
            DestroySpawnedWeapon(index);
            // TODO refactor collectible object to set the amount when getting the dropped item
            GameObject droppedItem = removedItem.item.GetDroppedGameObject(removedItem.count, player.transform.position);
            Vector2 throwDirection = player.GetComponent<PlayerMovement>().facingRight ? Vector2.right : Vector2.left;
            float throwForceX = 5f;  // Forward force
            float throwForceY = 1f;  // Upward force
            Vector2 throwForce = new Vector2(throwDirection.x * throwForceX, throwForceY);
            var rb = droppedItem.GetComponent<Rigidbody2D>();
            rb.AddForce(throwForce, ForceMode2D.Impulse);
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

    
    public void SwapItemsBetweenInventory(BaseInventory targetInventory, int index1, int index2)    
    {
        InventorySlot sourceSlot = this.GetInventorySlotAtIndex(index1);
        InventorySlot targetSlot = targetInventory.GetInventorySlotAtIndex(index2);
        if (targetInventory is WeaponInventory)
        {
            if (sourceSlot != null && sourceSlot.item is WeaponObject)
            {
                
                this.database.TransferItemToOtherInventory(targetInventory.database, index1, index2);
                UpdateSlotUi(index1);
                targetInventory.UpdateSlotUi(index2);
                targetInventory.SpawnWeaponIfAvailable();
            }
            else
            {
                UpdateSlotUi(index1);
                targetInventory.UpdateSlotUi(index2);
                return;
            }
        }
        else
        {
            if (sourceSlot != null && sourceSlot.item is WeaponObject)
            {
                if (targetSlot.item != null)
                {
                    UpdateSlotUi(index1);
                    targetInventory.UpdateSlotUi(index2);
                    return;
                }

                this.DestroySpawnedWeapon(index1);
            }
            this.database.TransferItemToOtherInventory(targetInventory.database, index1, index2);
            UpdateSlotUi(index1);
            targetInventory.UpdateSlotUi(index2);

        }
    }
    public virtual void DestroySpawnedWeapon(int slotIndex)
    {
        
    }
    public virtual void SpawnWeaponIfAvailable()
    {
            
    }


    public virtual void RemoveAllItemsAndDrops()
    {
        for (int i = 0; i < database.GetSize(); i++)
        {
            InventorySlot removedItem = database.RemoveItem(i);
            if (removedItem != null)
            {
                Vector3 dropPosition;
                if (player.GetComponent<PlayerMovement>().facingRight)
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

            UpdateSlotUi(i);
        }
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

    public InventoryDatabase Database
    {
        get { return database; }
    }


}
