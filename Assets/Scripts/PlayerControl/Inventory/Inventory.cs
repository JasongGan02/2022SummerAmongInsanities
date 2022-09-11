using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
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

    public GameObject defaultRow;
    public GameObject extraRow;
    public GameObject template;
    public GameObject background;

    public int defaultNumberOfRow = 2;
    public int maxExtraRow = 4;
    private int slotPerRow = 10;

    private int inventorySize;
    private int nextEmptySlotIndex = 0;

    private GameObject inventoryGrid;
    private GameObject hotbar;
    private readonly List<InventorySlot> inventory = new();

    private InventoryEventBus eventBus;

    void Awake()
    {
        inventorySize = defaultNumberOfRow * slotPerRow;
        inventoryGrid = GameObject.Find(Constants.Name.INVENTORY_GRID);

        int maxSize = (defaultNumberOfRow + maxExtraRow) * slotPerRow;
        for (int i = 0; i < maxSize; i++)
        {
            inventory.Add(new InventorySlot(null, 0));
        }

        SetupGrid();
        ToggleUi();
        eventBus = new InventoryEventBus();
        hotbar = inventoryGrid.transform.GetChild(0).gameObject;

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) {
            ToggleUi();
        }

        HandleHotbarKeyPress();
    }

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

    public int GetItemCountAtSlot(int index)
    {
        return inventory[index].count;
    }

    public InventorySlot GetInventorySlotAtIndex(int index)
    {
        return inventory[index];
    }

    public void AddItem(CollectibleObject item, int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            int slotIndex = inventory.FindIndex(slot => slot.item != null && slot.item.itemName == item.itemName && slot.count < item.maxStack);

            if (slotIndex == -1)
            {
                inventory[nextEmptySlotIndex].item = item;
                inventory[nextEmptySlotIndex].count++;
                UpdateSlotUi(nextEmptySlotIndex);
            }
            else
            {
                inventory[slotIndex].count++;
                UpdateSlotUi(slotIndex);
            }
            UpdateNextEmptySlot();
        }
    }

    public void RemoveItem(int index)
    {
        if (inventory[index].item == null) return;

        Vector3 dropPosition;
        if(gameObject.GetComponent<Playermovement>().facingRight)
        {
            dropPosition = gameObject.transform.position + new Vector3(1, 0, 0);
        }
        else
        {
            dropPosition = gameObject.transform.position + new Vector3(-1, 0, 0);
        }
        GameObject droppedItem = Instantiate(inventory[index].item.droppedItem, dropPosition, Quaternion.identity);
        droppedItem.GetComponent<DroppedObjectController>().amount = inventory[index].count;

        inventory[index].item = null;
        inventory[index].count = 0;

        UpdateSlotUi(index);
        UpdateNextEmptySlot();
    }

    public void SwapItems(int index1, int index2)
    {
        int tempCount = inventory[index2].count;
        CollectibleObject tempItem = inventory[index2].item;

        inventory[index2].item = inventory[index1].item;
        inventory[index2].count = inventory[index1].count;

        inventory[index1].item = tempItem;
        inventory[index1].count = tempCount;

        UpdateSlotUi(index1);
        UpdateSlotUi(index2);
        UpdateNextEmptySlot();
    }

    public bool MoveItems(int fromIndex, int toIndex, int amount)
    {
        if (inventory[fromIndex].item == null) return false;

        CollectibleObject item = inventory[fromIndex].item;
        int count = inventory[fromIndex].count;

        if (inventory[toIndex].item == null)
        {
            inventory[toIndex].item = item;
        } 
        else
        {
            Debug.Log(1);
            if (inventory[toIndex].item.name != inventory[fromIndex].item.name) return false;
            Debug.Log(2);
            if (amount + inventory[toIndex].count > inventory[toIndex].item.maxStack) return false;
            Debug.Log(3);
        }

        inventory[toIndex].count += amount;
        inventory[fromIndex].count -= amount;
        if (inventory[fromIndex].count == 0)
        {
            inventory[fromIndex].item = null;
        }

        UpdateSlotUi(toIndex);
        UpdateNextEmptySlot();
        return true;
    }

    private void UpdateNextEmptySlot()
    {
        nextEmptySlotIndex = FindNextEmptySlot();
    }

    private int FindNextEmptySlot()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventory[i].item == null)
            {
                return i;
            }
        }
        return -1;
    }

    public bool IsInventoryFull()
    {
        return nextEmptySlotIndex < 0;
    }

    private void UpdateSlotUi(int index)
    {
        GameObject slot = FindInventorySlotByIndex(index);

        foreach (Transform child in slot.transform)
        {
            Destroy(child.gameObject);
        }

        if (inventory[index].item != null)
        {
            Sprite icon = inventory[index].item.droppedItem.GetComponent<SpriteRenderer>().sprite;
            int count = inventory[index].count;

            GameObject slotContent = Instantiate(template);
            slotContent.transform.Find("Icon").GetComponent<Image>().sprite = icon;
            slotContent.GetComponent<ItemInteractionHandler>().collectibleItem = inventory[index].item;
            slotContent.transform.Find("Count").GetComponent<TMP_Text>().text = count.ToString();
            slotContent.transform.SetParent(slot.transform);
            slotContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        }
    }

    private GameObject FindInventorySlotByIndex(int index)
    {
        if (index >= inventorySize) return null;
        return inventoryGrid.transform.GetChild(index / 10).GetChild(index % 10).gameObject;
    }

    private void SetupGrid()
    {
        for (int i = 0; i < defaultNumberOfRow; i++)
        {
            GameObject row = Instantiate(defaultRow);
            row.transform.SetParent(inventoryGrid.transform);
            RectTransform rowRectTransform = row.GetComponent<RectTransform>();
            rowRectTransform.anchoredPosition = new Vector2(600, -50 + 120 * i);
        }

        int slotIndex = 0;
        for (int i = 0; i < inventoryGrid.transform.childCount; i++)
        {
            GameObject row = inventoryGrid.transform.GetChild(i).gameObject;
            for (int j = 0; j < row.transform.childCount; j++)
            {
                GameObject slot = row.transform.GetChild(j).gameObject;
                slot.name = "slot" + slotIndex++;
            }
        }

        /*GameObject background = Instantiate(this.background);
        RectTransform backgroundRectTransform = background.GetComponent<RectTransform>();
        background.GetComponent<RectTransform>().sizeDelta = new Vector2(backgroundRectTransform.sizeDelta.x, 120 * defaultNumberOfRow);
        background.transform.SetParent(inventoryGrid.transform);
        backgroundRectTransform.anchoredPosition = new Vector2(600, -50);*/
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

        GameObject slot = hotbar.transform.GetChild(index).gameObject;
        if (slot.transform.childCount > 0)
        {
            eventBus.OnSlotLeftClicked(index);
        }
    }

    private void ToggleUi()
    {
        bool isActive = inventoryGrid.transform.GetChild(1).gameObject.activeSelf;
        for(int i = 1; i < inventoryGrid.transform.childCount; i++)
        {
            inventoryGrid.transform.GetChild(i).gameObject.SetActive(!isActive);
        }
       
        PlayerStatusRepository.SetIsViewingUi(!isActive);
    }
}


