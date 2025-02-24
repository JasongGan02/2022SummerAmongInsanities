using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Lumin;
using UnityEngine.Experimental.Rendering;

public class Inventory : BaseInventory, Inventory.InventoryButtonClickedCallback
{
    public GameObject extraRow;
    public int maxExtraRow = 4;
    private GameObject hotBar;
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
            FindFirstObjectByType<UIViewStateManager>()
        );

        uiController.SetupUi();
        hotBar = inventoryGrid.transform.GetChild(0).gameObject;

        eventBus = new InventoryEventBus();
        queueManager = FindFirstObjectByType<CraftingQueueManager>();
    }

    protected override void Update()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        HandleHotbarKeyPress();
    }

    public void AddItem(IInventoryObject item, int amount)
    {
        for (int i = 0; i < amount; i++)
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

    public void CraftItems(CraftRecipe[] recipe, BaseObject outputItem)
    {
        if (queueManager.sizeCraftQueue() >= 10)
        {
            Debug.Log("CraftItems: Craft queue is full (>=4). Aborting craft.");
            return;
        }

        // First, check if all items are available in required quantities
        if (!CheckRecipeHasEnoughMaterials(recipe)) return;

        // Since all items are available, consume them
        for (int i = 0; i < recipe.Length; i++)
        {
            ConsumeItem(recipe[i].material as IInventoryObject, recipe[i].quantity);
        }

        // Add the output item to the crafting queue
        queueManager.AddToQueue(outputItem, () => { AddItem(outputItem as IInventoryObject, 1); });
    }

    public bool CheckRecipeHasEnoughMaterials(CraftRecipe[] recipe)
    {
        return recipe.All(craftRecipe => HasEnoughItem((IInventoryObject)craftRecipe.material, craftRecipe.quantity));
    }

    private bool HasEnoughItem(IInventoryObject inventoryObject, int requiredQuantity)
    {
        if (inventoryObject == null)
        {
            Debug.LogError("HasEnoughItem: inventoryObject is null!");
            return false;
        }

        int totalFound = 0;
        for (int i = 0; i < database.GetSize(); i++)
        {
            InventorySlot slot = database.GetInventorySlotAtIndex(i);
            if (!slot.IsEmpty &&
                slot.item is BaseObject slotBaseObj &&
                inventoryObject is BaseObject recipeBaseObj &&
                slotBaseObj.itemName == recipeBaseObj.itemName)
            {
                //Debug.LogError($"Found {slot.count} of {inventoryObject.GetItemName()} in slot {i}.");
                totalFound += slot.count;
            }
        }

        //Debug.LogError($"Total found of {inventoryObject.GetItemName()} = {totalFound}, required = {requiredQuantity}");
        return totalFound >= requiredQuantity;
    }


    public bool CanAddItem(IInventoryObject item)
    {
        for (int i = 0; i < database.GetSize(); i++)
        {
            InventorySlot slot = database.GetInventorySlotAtIndex(i);

            // Check if slot contains the same item type and is not full
            if (slot != null && !slot.IsEmpty && (slot.item as BaseObject) == (item as BaseObject))
            {
                if (slot.count < slot.item.MaxStack)
                {
                    return true;
                }
            }
        }

        // Check if there's an empty slot
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
    }


    private void UseItemInHotbarSlot(int index)
    {
        if (index > 7) return;

        GameObject slot = hotBar.transform.GetChild(0).GetChild(index).gameObject;
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


    public bool ConsumeItem(IInventoryObject inventoryObject, int amountNeeded)
    {
        if (inventoryObject == null)
        {
            Debug.LogError("ConsumeItem: inventoryObject is null!");
            return false;
        }

        int remaining = amountNeeded;

        // Loop over all slots, removing items until we've removed everything
        for (int i = 0; i < database.GetSize(); i++)
        {
            if (remaining <= 0) break; // Done removing

            InventorySlot slot = database.GetInventorySlotAtIndex(i);
            if (!slot.IsEmpty &&
                slot.item is BaseObject slotBaseObj &&
                inventoryObject is BaseObject recipeBaseObj &&
                slotBaseObj.itemName == recipeBaseObj.itemName)
            {
                int canRemove = Mathf.Min(slot.count, remaining);
                //Debug.LogError($"ConsumeItem: Removing {canRemove} from slot {i}. (Slot had {slot.count}, need {remaining} total left)");
                database.RemoveItemByOne(i, canRemove);
                UpdateSlotUi(i);

                remaining -= canRemove;
            }
        }

        if (remaining > 0)
        {
            // We still haven't removed everything. Means we didn't have enough items across all slots.
            //Debug.LogError($"ConsumeItem: After checking all slots, still need {remaining} more of {inventoryObject.GetItemName()}!");
            return false;
        }

        return true;
    }


    public InventorySlot FindSlot(IInventoryObject inventoryObject)
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

    public int FindSlotindex(IInventoryObject inventoryObject)
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

    public int FindSlotIndex(string inventoryObject)
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


    public int FindItemCount(BaseObject inventoryObject)
    {
        // Ensure that inventoryObject is not null
        if (inventoryObject == null)
        {
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