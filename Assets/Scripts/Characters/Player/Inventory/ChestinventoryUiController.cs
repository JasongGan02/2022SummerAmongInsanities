using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestinventoryUiController : InventoryUiController
{
   
    public ChestinventoryUiController(
    int defaultNumberOfRow,
    GameObject defaultRow,
    GameObject inventoryGrid,
    GameObject template,
    Inventory.InventoryButtonClickedCallback buttonClickedCallback,
    UIViewStateManager uiViewStateManager
    ) : base(defaultNumberOfRow, defaultRow, null, inventoryGrid, template, buttonClickedCallback, uiViewStateManager)
    {
        this.defaultNumberOfRow = defaultNumberOfRow;
        this.defaultRow = defaultRow;
        this.inventoryGrid = inventoryGrid;
        this.template = template;
        this.buttonClickedCallback = buttonClickedCallback;

        this.inventoryContainer = this.inventoryGrid.transform.Find("Inventory").gameObject;
        this.actionsContainer = this.inventoryGrid.transform.Find("Actions").gameObject;

        this.uiViewStateManager = uiViewStateManager;

        //this.uiViewStateManager.UpdateUiBeingViewedEvent += UpdateInventoryUi;
    }

    public override void SetupUi()
    {
        for (int i = 0; i < defaultNumberOfRow - 1; i++)
        {
            GameObject row = GameObject.Instantiate(defaultRow);
            for (int j = 0; j < row.transform.childCount; j++)
            {
                row.transform.GetChild(j).gameObject.name = "slot" + slotIndex++;
            }
            row.transform.SetParent(inventoryContainer.transform);
            RectTransform rowRectTransform = row.GetComponent<RectTransform>();
            rowRectTransform.anchoredPosition = new Vector2(1000, -300 + 90 * i);
        }
        Button sortButton = actionsContainer.transform.Find("Sort").GetComponent<Button>();
        sortButton.onClick.AddListener(OnSortButtonClicked);
        SetUiActive(false);
    }




}
