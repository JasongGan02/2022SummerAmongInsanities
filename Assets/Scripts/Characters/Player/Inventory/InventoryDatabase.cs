using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.VolumeComponent;

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
    public InventoryDatabase(int defaultNumberOfRow,int maxExtraRow, int slotPerRow)
    {
        maxSize = (defaultNumberOfRow + maxExtraRow) * slotPerRow;
        size = defaultNumberOfRow * slotPerRow;
        for (int i = 0; i < maxSize; i++)
        {
            inventory.Add(new InventorySlot(null, 0));
        }
    }

    public InventoryDatabase(int defaultNumberOfRow, IInventoryObject defaultInventoryObject, int maxExtraRow)
    {
        maxSize = (defaultNumberOfRow + maxExtraRow) * slotPerRow;
        size = defaultNumberOfRow * slotPerRow;
        for (int i = 0; i < maxSize; i++)
        {
            inventory.Add(new InventorySlot(defaultInventoryObject, 0));
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

    public int GetSize()
    {
        return size;
    }

    // return the slot index that needs to be updated
    public int AddItem(IInventoryObject item)
    {
        int slotIndex = inventory.FindIndex(slot => slot.item != null && slot.item.GetItemName() == item.GetItemName() && slot.count < item.MaxStack);
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

    public void RemoveItemByOne(int index)
    {
        if (inventory[index].item == null) return;

        inventory[index].count -= 1;
        if (inventory[index].count == 0)
        {
            inventory[index].item = null;
            UpdateNextEmptySlot();
        }
    }

    public void RemoveItemByOne(int index, int num)
    {
        if (inventory[index].item == null) return;

        if(inventory[index].count <num) return;
        
        inventory[index].count -= num;
        if (inventory[index].count <= 0)
        {
            inventory[index].item = null;
            UpdateNextEmptySlot();
        }
    }

    public void SwapItems(int index1, int index2)
    {
        int tempCount = inventory[index2].count;
        IInventoryObject tempItem = inventory[index2].item;

        inventory[index2].item = inventory[index1].item;
        inventory[index2].count = inventory[index1].count;

        inventory[index1].item = tempItem;
        inventory[index1].count = tempCount;

        UpdateNextEmptySlot();
    }

    // return the amount of not moved items
    public int MoveItems(int fromIndex, int toIndex, int amount)
    {
        if (inventory[fromIndex].item == null) return -1;
        if (inventory[fromIndex].count < amount) return -1;

        // TODO take care of when the case where dropped amount + existing amount is larger than the max stack
        if (inventory[toIndex].item == null)
        {
            inventory[toIndex].item = inventory[fromIndex].item;
        }
        else
        {
            if (inventory[toIndex].item.GetItemName() != inventory[fromIndex].item.GetItemName()) return amount;
            //if (amount + inventory[toIndex].count > inventory[toIndex].item.maxStack) return false;
        }

        int remainingItemCount = inventory[toIndex].count + amount - inventory[toIndex].item.MaxStack;
        int movedItemCount = amount - remainingItemCount;
        if (remainingItemCount > 0)
        {
            inventory[toIndex].count = inventory[toIndex].item.MaxStack;
            inventory[fromIndex].count -= movedItemCount;
            
        } 
        else
        {
            inventory[toIndex].count += amount;
            inventory[fromIndex].count -= amount;
            if (inventory[fromIndex].count == 0)
            {
                inventory[fromIndex].item = null;
            }
        }

        UpdateNextEmptySlot();
        return inventory[fromIndex].count;
    }

    public bool CanUpgrade()
    {
        return maxSize > size;
    }

    public void Upgrade()
    {
        size += 10;
        UpdateNextEmptySlot();
    }

    public void Sort()
    {
        MergeItems();
        inventory.Sort();
        UpdateNextEmptySlot();
    }

    public bool HasEmptySlot()
    {
        return nextEmptySlotIndex >= 0;
    }

    private void MergeItems()
    {
        HashSet<String> visited = new();

        for (int i = size - 1; i >= 0; i--)
        {
            if (inventory[i].IsEmpty || visited.Contains(inventory[i].item.GetItemName())) continue;
            String itemName = inventory[i].item.GetItemName();
            visited.Add(itemName);

            // find all indices of the same item type whose slot is not full
            List<int> mergeList = new();
            for (int j = 0; j <= i; j++)
            {
                InventorySlot slot = inventory[j];
                if (!slot.IsEmpty && slot.item.GetItemName() == itemName && slot.count < slot.item.MaxStack)
                {
                    mergeList.Add(j);
                }
            }

            // merge items in the merge list
            int totalCount = 0;
            int maxStack = inventory[i].item.MaxStack;
            mergeList.ForEach(index => totalCount += inventory[index].count);
            foreach (int index in mergeList)
            {
                if (totalCount == 0)
                {
                    inventory[index].item = null;
                    inventory[index].count = 0;
                } 
                else if (totalCount - maxStack >= 0)
                {
                    inventory[index].count = maxStack;
                    totalCount -= maxStack;
                } 
                else
                {
                    inventory[index].count = totalCount;
                    totalCount = 0;
                }
            }

        }
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

    public void TransferItemToOtherInventory(InventoryDatabase otherInventory, int fromIndex, int toIndex)
    {
        InventorySlot sourceSlot = GetInventorySlotAtIndex(fromIndex);
        InventorySlot targetSlot = otherInventory.GetInventorySlotAtIndex(toIndex);

        // Ensure there is an item to transfer.
        if (sourceSlot.item == null || sourceSlot.count == 0)
        {
            return; // Nothing to transfer.
        }

        // If the target slot is empty, move the item directly.
        if (targetSlot.IsEmpty)
        {
            otherInventory.inventory[toIndex] = new InventorySlot(sourceSlot.item, sourceSlot.count);
            this.inventory[fromIndex].item = null;
            this.inventory[fromIndex].count = 0;
        }
        else if (targetSlot.item.GetItemName() == sourceSlot.item.GetItemName())
        {
            // If the items are the same, stack them, respecting the max stack size.
            int availableSpace = targetSlot.item.MaxStack - targetSlot.count;
            int transferAmount = Math.Min(sourceSlot.count, availableSpace);

            otherInventory.inventory[toIndex].count += transferAmount;
            this.inventory[fromIndex].count -= transferAmount;

            // If all items are transferred, clear the source slot.
            if (this.inventory[fromIndex].count == 0)
            {
                this.inventory[fromIndex].item = null;
            }
        }
        else
        {
            
            //swap the items between the two slots.
            int tempCount = otherInventory.inventory[toIndex].count;
            IInventoryObject tempItem = otherInventory.inventory[toIndex].item;

            otherInventory.inventory[toIndex].item = inventory[fromIndex].item;
            otherInventory.inventory[toIndex].count = inventory[fromIndex].count;

            inventory[fromIndex].item = tempItem;
            inventory[fromIndex].count = tempCount;
        }
        UpdateNextEmptySlot();

    }



}

public class InventorySlot : IComparable
{
    public IInventoryObject item;
    public int count;

    public bool IsEmpty { get => item == null; }

    public InventorySlot(IInventoryObject item, int count)
    {
        this.item = item;
        this.count = count;
    }

    public InventorySlot(IInventoryObject item)
    {
        this.item = item;
        this.count = 1;
    }

    public int CompareTo(object obj)
    {
        if (obj == null) return -1;

        InventorySlot otherSlot = obj as InventorySlot;
        if (otherSlot != null)
        {
            if (otherSlot.IsEmpty && this.IsEmpty) return 0;
            if (otherSlot.IsEmpty) return -1;
            if (this.IsEmpty) return 1;

            if (otherSlot.item.GetItemName() == this.item.GetItemName())
            {
                return otherSlot.count - this.count;
            }
            else
            {
                return this.item.GetItemName().CompareTo(otherSlot.item.GetItemName());
            }
        } 
        else
        {
            throw new ArgumentException("Object is not a InventorySlot");
        }
    }
}
