using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUiController
{
    public int slotIndex = 0;

    private int defaultNumberOfRow;
    private GameObject defaultRow;
    private GameObject inventoryGrid;
    private GameObject template;
    private Inventory.InventoryButtonClickedCallback buttonClickedCallback;
    private BaseInventory.IInventoryButtonClickedCallback BasebuttonClickedCallback;

    private GameObject extraRow;
    private GameObject hotbarContainer;

    private GameObject inventoryContainer;
    private GameObject actionsContainer;
    protected UIViewStateManager uiViewStateManager;

    public InventoryUiController(
        int defaultNumberOfRow, 
        GameObject defaultRow,
        GameObject extraRow,
        GameObject inventoryGrid,
        GameObject template,
        Inventory.InventoryButtonClickedCallback buttonClickedCallback,
        UIViewStateManager uiViewStateManager
        )
    {
        this.defaultNumberOfRow = defaultNumberOfRow;
        this.defaultRow = defaultRow;
        this.extraRow = extraRow;
        this.inventoryGrid = inventoryGrid;
        this.template = template;
        this.buttonClickedCallback = buttonClickedCallback;

        this.hotbarContainer = this.inventoryGrid.transform.Find("Hotbar").gameObject;
        this.inventoryContainer = this.inventoryGrid.transform.Find("Inventory").gameObject;
        this.actionsContainer = this.inventoryGrid.transform.Find("Actions").gameObject;

        this.uiViewStateManager = uiViewStateManager;

        this.uiViewStateManager.UpdateUiBeingViewedEvent += UpdateInventoryUi;
    }

    public InventoryUiController(
        int defaultNumberOfRow,
        GameObject defaultRow,
        GameObject inventoryGrid,
        GameObject template,
        BaseInventory.IInventoryButtonClickedCallback BasebuttonClickedCallback,
        UIViewStateManager uiViewStateManager
        )
    {
        this.defaultNumberOfRow = defaultNumberOfRow;
        this.defaultRow = defaultRow;
        this.inventoryGrid = inventoryGrid;
        this.template = template;
        this.BasebuttonClickedCallback = BasebuttonClickedCallback;

        this.inventoryContainer = this.inventoryGrid.transform.Find("ChestInventory").gameObject;
        this.actionsContainer = this.inventoryGrid.transform.Find("ChestActions").gameObject;

        this.uiViewStateManager = uiViewStateManager;

        this.uiViewStateManager.UpdateUiBeingViewedEvent += UpdateChestInventoryUi;
    }

    public InventoryUiController(
        int defaultNumberOfRow,
        GameObject defaultRow,
        GameObject inventoryGrid,
        GameObject template,
        UIViewStateManager uiViewStateManager
        )
    {
        this.defaultNumberOfRow = defaultNumberOfRow;
        this.defaultRow = defaultRow;
        this.inventoryGrid = inventoryGrid;
        this.template = template;

        this.inventoryContainer = this.inventoryGrid.transform.Find("WeaponInventory").gameObject;

        this.uiViewStateManager = uiViewStateManager;

    }


    ~InventoryUiController()
    {
        this.uiViewStateManager.UpdateUiBeingViewedEvent -= UpdateInventoryUi;
        this.uiViewStateManager.UpdateUiBeingViewedEvent -= UpdateChestInventoryUi;

    }

    public virtual void SetupUi()
    {
        {
            GameObject hotbar = GameObject.Instantiate(defaultRow);
            for (int i = 0; i < hotbar.transform.childCount; i++)
            {
                hotbar.transform.GetChild(i).gameObject.name = "slot" + slotIndex++;
            }
            hotbar.transform.SetParent(hotbarContainer.transform);
            RectTransform rowRectTransform = hotbar.GetComponent<RectTransform>();
            rowRectTransform.anchoredPosition = new Vector2(450,-30);
        }

        for (int i = 0; i < defaultNumberOfRow - 1; i++)
        {
            GameObject row = GameObject.Instantiate(defaultRow);
            for (int j = 0; j < row.transform.childCount; j++)
            {
                row.transform.GetChild(j).gameObject.name = "slot" + slotIndex++;
            }
            row.transform.SetParent(inventoryContainer.transform);
            RectTransform rowRectTransform = row.GetComponent<RectTransform>();
            rowRectTransform.anchoredPosition = new Vector2(450, 90 * i);
        }

        Button sortButton = actionsContainer.transform.Find("Sort").GetComponent<Button>();
        Button upgradeButton = actionsContainer.transform.Find("Upgrade").GetComponent<Button>();

        sortButton.onClick.AddListener(OnSortButtonClicked);
        upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);

        SetUiActive(false);
    }

    public void SetupChestUi()
    {
        for (int i = 0; i < defaultNumberOfRow; i++)
        {
            GameObject row = GameObject.Instantiate(defaultRow);
            for (int j = 0; j < row.transform.childCount; j++)
            {
                row.transform.GetChild(j).gameObject.name = "slot" + slotIndex++;
            }
            row.transform.SetParent(inventoryContainer.transform);
            RectTransform rowRectTransform = row.GetComponent<RectTransform>();
            // rowRectTransform.anchoredPosition = new Vector2(500, -200 - 90 * i);
        }
        Button sortButton = actionsContainer.transform.Find("ChestSort").GetComponent<Button>();
        sortButton.onClick.AddListener(OnChestSortButtonClicked);
        SetChestUiActive(false);
    }

    public void SetupWeaponUi()
    {
        GameObject row = GameObject.Instantiate(defaultRow);
        for (int j = 0; j < 2; j++) // Only two slots for weapons
        {
            GameObject slot = row.transform.GetChild(j).gameObject;
            slot.name = "slot" + j;
            slotIndex++;
        }
        row.transform.SetParent(inventoryContainer.transform);
        RectTransform rowRectTransform = row.GetComponent<RectTransform>();
        rowRectTransform.anchoredPosition = new Vector2(100, -40);
        SetWeaponUiActive(true);
    }


    public void UpdateSlotUi(int index, InventorySlot slot)
    {
        
        GameObject slotUi = GetInventorySlotUiByIndex(index);

        foreach (Transform child in slotUi.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        if (!slot.IsEmpty)
        {
            GameObject slotContent = GameObject.Instantiate(template);
            slotContent.transform.Find("Icon").GetComponent<Image>().sprite = slot.item.GetSpriteForInventory();
            slotContent.GetComponent<ItemInteractionHandler>().item = slot.item;
            slotContent.transform.Find("Count").GetComponent<TMP_Text>().text = slot.count.ToString();
            slotContent.transform.SetParent(slotUi.transform);
            slotContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        }
    }


    private void UpdateInventoryUi(object sender, UIBeingViewed ui)
    {
        bool isActive = ui == UIBeingViewed.Inventory || ui == UIBeingViewed.Chest;

        SetUiActive(isActive, ui == UIBeingViewed.Inventory || ui == UIBeingViewed.Null);
    }

    private void UpdateChestInventoryUi(object sender, UIBeingViewed ui)
    {
        bool isActive = ui == UIBeingViewed.Chest;

        SetChestUiActive(isActive);
    }




    public void SetUiActive(bool isInventoryActive, bool isHotbarActive = true)
    {
        inventoryContainer.SetActive(isInventoryActive);
        actionsContainer.SetActive(isInventoryActive);

        PlayerStatusRepository.SetIsViewingUi(isInventoryActive);

        hotbarContainer.SetActive(true);    
    }


    public void SetChestUiActive(bool isChestInventoryActive)
    {
        PlayerStatusRepository.SetIsViewingUi(isChestInventoryActive);
        inventoryContainer.SetActive(isChestInventoryActive);
        actionsContainer.SetActive(isChestInventoryActive);
    }

    public void SetWeaponUiActive(bool isWeaponInventoryActive)
    {
        PlayerStatusRepository.SetIsViewingUi(isWeaponInventoryActive);
        inventoryContainer.SetActive(isWeaponInventoryActive);
    }


    /*
     public void UpdateDatabase(InventoryDatabase newDatabase)
     {
         // Assuming each row has a fixed number of slots, which is the child count of a row
         int slotsPerRow = defaultRow.transform.childCount;

         // Iterate over each slot in the new database and update the UI
         for (int i = 0; i < newDatabase.GetSize(); i++)
         {
             // Calculate the row and position in the row for the current slot
             int row = i / slotsPerRow;
             int positionInRow = i % slotsPerRow;

             // Find the corresponding UI slot GameObject
             GameObject rowGameObject = inventoryContainer.transform.GetChild(row).gameObject;
             GameObject slotUi = rowGameObject.transform.GetChild(positionInRow).gameObject;

             // Update the slot UI with the item from the new database
             UpdateSlotUiFromDatabase(slotUi, newDatabase.GetInventorySlotAtIndex(i));
         }
     }

     // Helper method to update a single slot UI from an inventory slot
     private void UpdateSlotUiFromDatabase(GameObject slotUi, InventorySlot slot)
     {
         // Clear existing slot contents
         foreach (Transform child in slotUi.transform)
         {
             GameObject.Destroy(child.gameObject);
         }

         // If the slot is not empty, create and position the new item UI
         if (!slot.IsEmpty)
         {
             GameObject slotContent = GameObject.Instantiate(template);
             slotContent.transform.SetParent(slotUi.transform, false);
             slotContent.transform.Find("Icon").GetComponent<Image>().sprite = slot.item.GetSpriteForInventory();
             slotContent.GetComponent<ItemInteractionHandler>().item = slot.item;
             slotContent.transform.Find("Count").GetComponent<TMP_Text>().text = slot.count.ToString();
         }
     }

     */

    public void Upgrade()
    {
        GameObject row = GameObject.Instantiate(extraRow);
        for (int i = 0; i < row.transform.childCount; i++)
        {
            row.transform.GetChild(i).gameObject.name = "slot" + slotIndex++;
        }
        row.transform.SetParent(inventoryContainer.transform);
        RectTransform rowRectTransform = row.GetComponent<RectTransform>();
        rowRectTransform.anchoredPosition = new Vector2(450, - 90 * (inventoryContainer.transform.childCount - 1));
    }

    private GameObject GetInventorySlotUiByIndex(int index)
    {
        if (hotbarContainer == null)
        {
            return inventoryContainer.transform.GetChild(index / 10).GetChild(index % 10).gameObject;
        }
        else
        {
            if (index < 10)
            {
                return hotbarContainer.transform.GetChild(index / 10).GetChild(index % 10).gameObject;
            }
            else
            {
                return inventoryContainer.transform.GetChild(index / 10 - 1).GetChild(index % 10).gameObject;
            }
        }
    }
    private GameObject GetChestInventorySlotUiByIndex(int index)
    {
            return inventoryContainer.transform.GetChild(index / 10).GetChild(index % 10).gameObject;
    }


    private void OnSortButtonClicked()
    {
        buttonClickedCallback.Sort();
    }

    private void OnChestSortButtonClicked()
    {
        BasebuttonClickedCallback.Sort();
    }

    private void OnUpgradeButtonClicked()
    {
        buttonClickedCallback.Upgrade();
    }
}
