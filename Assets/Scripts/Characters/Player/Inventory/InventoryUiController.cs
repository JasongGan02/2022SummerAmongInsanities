using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUiController
{
    private int slotIndex = 0;

    private int defaultNumberOfRow;
    private GameObject defaultRow;
    private GameObject extraRow;
    private GameObject inventoryGrid;
    private GameObject template;
    private Inventory.InventoryButtonClickedCallback buttonClickedCallback;
    private BaseInventory.InventoryButtonClickedCallback BaseButtonClickedCallback;

    private GameObject hotbarContainer;
    private GameObject inventoryContainer;
    private GameObject actionsContainer;

    private UIViewStateManager uiViewStateManager;

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
        BaseInventory.InventoryButtonClickedCallback BasebuttonClickedCallback,
        UIViewStateManager uiViewStateManager
        )
    {
        this.defaultNumberOfRow = defaultNumberOfRow;
        this.defaultRow = defaultRow;
        this.inventoryGrid = inventoryGrid;
        this.template = template;
        this.BaseButtonClickedCallback = BasebuttonClickedCallback;

        this.hotbarContainer = this.inventoryGrid.transform.Find("Hotbar").gameObject;
        this.inventoryContainer = this.inventoryGrid.transform.Find("Inventory").gameObject;
        this.actionsContainer = this.inventoryGrid.transform.Find("Actions").gameObject;

        this.uiViewStateManager = uiViewStateManager;

        this.uiViewStateManager.UpdateUiBeingViewedEvent += UpdateInventoryUi;
    }




    ~InventoryUiController()
    {
        this.uiViewStateManager.UpdateUiBeingViewedEvent -= UpdateInventoryUi;
    }

    public void SetupUi()
    {
        {
            GameObject hotbar = GameObject.Instantiate(defaultRow);
            for (int i = 0; i < hotbar.transform.childCount; i++)
            {
                hotbar.transform.GetChild(i).gameObject.name = "slot" + slotIndex++;
            }
            hotbar.transform.SetParent(hotbarContainer.transform);
            RectTransform rowRectTransform = hotbar.GetComponent<RectTransform>();
            rowRectTransform.anchoredPosition = new Vector2(600, -50);
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
            rowRectTransform.anchoredPosition = new Vector2(600, -50 + 120 * i);
        }

        Button sortButton = actionsContainer.transform.Find("Sort").GetComponent<Button>();
        Button upgradeButton = actionsContainer.transform.Find("Upgrade").GetComponent<Button>();

        sortButton.onClick.AddListener(OnSortButtonClicked);
        upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);

        SetUiActive(false);
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
        bool isActive = ui == UIBeingViewed.Inventory;
        
        SetUiActive(isActive, ui == UIBeingViewed.Inventory || ui == UIBeingViewed.Null);
    }

    public void SetUiActive(bool isInventoryActive, bool isHotbarActive = true)
    {
        inventoryContainer.SetActive(isInventoryActive);
        actionsContainer.SetActive(isInventoryActive);

        PlayerStatusRepository.SetIsViewingUi(isInventoryActive);

        hotbarContainer.SetActive(true);
    }

    public void Upgrade()
    {
        GameObject row = GameObject.Instantiate(extraRow);
        for (int i = 0; i < row.transform.childCount; i++)
        {
            row.transform.GetChild(i).gameObject.name = "slot" + slotIndex++;
        }
        row.transform.SetParent(inventoryContainer.transform);
        RectTransform rowRectTransform = row.GetComponent<RectTransform>();
        rowRectTransform.anchoredPosition = new Vector2(600, -50 - 120 * (inventoryContainer.transform.childCount - 1));
    }

    private GameObject GetInventorySlotUiByIndex(int index)
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

    private void OnSortButtonClicked()
    {
        buttonClickedCallback.Sort();
    }

    private void OnUpgradeButtonClicked()
    {
        buttonClickedCallback.Upgrade();
    }
}
