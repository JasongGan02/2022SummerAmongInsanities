using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryDatabase
{
    private readonly List<InventorySlot> inventory = new();

    private int slotPerRow = 10;

    private int size;
    private int maxSize;
    private int nextEmptySlotIndex = 0;

    public InventoryDatabase(int defaultNumberOfRow, int maxExtraRow)
    {
        maxSize = (defaultNumberOfRow + maxExtraRow) * slotPerRow;
        size = defaultNumberOfRow * slotPerRow;
        for (int i = 0; i < maxSize; i++)
        {
            inventory.Add(new InventorySlot(null, 0));
        }
    }

    public InventorySlot GetInventorySlotAtIndex(int index)
    {
        return inventory[index];
    }

    public bool IsSlotEmpty(int index)
    {
        return inventory[index].IsEmpty;
    }

    // return the slot index that needs to be updated
    public int AddItem(CollectibleObject item)
    {
        int slotIndex = inventory.FindIndex(slot => slot.item != null && slot.item.itemName == item.itemName && slot.count < item.maxStack);
        int indexToUpdate;
        if (slotIndex == -1)
        {
            // TODO return -1 if no more space in the inventory
            inventory[nextEmptySlotIndex].item = item;
            inventory[nextEmptySlotIndex].count++;
            indexToUpdate = nextEmptySlotIndex;
        }
        else
        {
            inventory[slotIndex].count++;
            indexToUpdate = slotIndex;
        }
        UpdateNextEmptySlot();
        return indexToUpdate;
    }

    // return the removed item
    public InventorySlot RemoveItem(int index)
    {
        if (inventory[index].item == null) return null;

        InventorySlot slot = new InventorySlot(inventory[index].item, inventory[index].count);

        inventory[index].item = null;
        inventory[index].count = 0;

        UpdateNextEmptySlot();
        return slot;
    }

    public void SwapItems(int index1, int index2)
    {
        int tempCount = inventory[index2].count;
        CollectibleObject tempItem = inventory[index2].item;

        inventory[index2].item = inventory[index1].item;
        inventory[index2].count = inventory[index1].count;

        inventory[index1].item = tempItem;
        inventory[index1].count = tempCount;

        UpdateNextEmptySlot();
    }

    public bool MoveItems(int fromIndex, int toIndex, int amount)
    {
        if (inventory[fromIndex].item == null) return false;

        CollectibleObject item = inventory[fromIndex].item;
        int count = inventory[fromIndex].count;

        // TODO take care of when the case where dropped amount + existing amount is larger than the max stack
        if (inventory[toIndex].item == null)
        {
            inventory[toIndex].item = item;
        }
        else
        {
            if (inventory[toIndex].item.name != inventory[fromIndex].item.name) return false;
            if (amount + inventory[toIndex].count > inventory[toIndex].item.maxStack) return false;
        }

        inventory[toIndex].count += amount;
        inventory[fromIndex].count -= amount;
        if (inventory[fromIndex].count == 0)
        {
            inventory[fromIndex].item = null;
        }

        UpdateNextEmptySlot();
        return true;
    }

    public bool hasEmptySlot()
    {
        return nextEmptySlotIndex >= 0;
    }

    private void UpdateNextEmptySlot()
    {
        nextEmptySlotIndex = FindNextEmptySlot();
    }

    private int FindNextEmptySlot()
    {
        for (int i = 0; i < size; i++)
        {
            if (inventory[i].item == null)
            {
                return i;
            }
        }
        return -1;
    }
}

public class InventorySlot
{
    public CollectibleObject item;
    public int count;
    public bool IsEmpty { get => item == null; }

    public InventorySlot(CollectibleObject item, int count)
    {
        this.item = item;
        this.count = count;
    }

    public InventorySlot(CollectibleObject item)
    {
        this.item = item;
        this.count = 1;
    }
}
