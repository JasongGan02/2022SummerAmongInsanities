using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
    private class InventorySlot
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

    public GameObject extraRow;
    public GameObject template;

    public int defaultNumberOfRow = 4;
    public int extraNumberOfRow = 2;
    // TODO: can't add item when inventory is full
    private int inventorySize;

    public GameObject inventoryUi;
    private GameObject grid;
    private PlayerStatusRepository playerStatusRepository;
    private readonly List<InventorySlot> inventory = new();

    void Awake()
    {
        inventorySize = (defaultNumberOfRow + extraNumberOfRow) * 10;
        inventoryUi = GameObject.Find(Constants.Name.INVENTORY_UI);
        grid = GameObject.Find(Constants.Name.INVENTORY_GRID);
        playerStatusRepository = GameObject.Find(Constants.Name.PLAYER).GetComponent<PlayerStatusRepository>();
        inventoryUi.SetActive(false);

        SetupGrid();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) {
            ToggleUi();
        }
    }

    public void AddItem(CollectibleObject item)
    {
        int slotIndex = inventory.FindIndex(slot => slot.item.itemName == item.itemName && slot.count < item.maxStack);
        
        if (slotIndex == -1)
        {
            inventory.Add(new InventorySlot(item, 1));
            UpdateSlot(inventory.Count - 1);
        }
        else
        {
            InventorySlot slot = inventory[slotIndex];
            slot.count++;
            UpdateSlot(slotIndex);
            Debug.Log(slot.count);
        }
    }

    private void UpdateSlot(int index)
    {
        GameObject slot = FindSlotUiByIndex(index);
        Sprite icon = inventory[index].item.droppedItem.GetComponent<SpriteRenderer>().sprite;
        int count = inventory[index].count;
        foreach (Transform child in slot.transform)
        {
            Destroy(child.gameObject);
        }
        GameObject slotContent = Instantiate(template);
        slot.transform.position = new Vector2(0, 0);
        slotContent.transform.Find("Icon").GetComponent<Image>().sprite = icon;
        slotContent.transform.Find("Count").GetComponent<TMP_Text>().text = count.ToString();
        slotContent.transform.SetParent(slot.transform);
    }

    private GameObject FindSlotUiByIndex(int index)
    {
        return grid.transform.GetChild(index / 10).GetChild(index % 10).gameObject;
    }

    private void SetupGrid()
    {
        for (int i = 0; i < extraNumberOfRow; i++)
        {
            GameObject row = Instantiate(extraRow);
            row.transform.SetParent(grid.transform);
            RectTransform rectTransform = grid.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y + 120);
        }

        int slotIndex = 0;
        for (int i = 0; i < grid.transform.childCount; i++)
        {
            GameObject row = grid.transform.GetChild(i).gameObject;
            for (int j = 0; j < row.transform.childCount; j++)
            {
                GameObject slot = row.transform.GetChild(j).gameObject;
                slot.name = "slot" + slotIndex++;
            }
        }
    }

    private void ToggleUi()
    {
        bool isActive = inventoryUi.activeSelf;
        inventoryUi.SetActive(!isActive);
        playerStatusRepository.SetIsViewingUi(!isActive);
    }
}


