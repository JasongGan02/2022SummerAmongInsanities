using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUiController
{
    private int defaultNumberOfRow;
    private GameObject defaultRow;
    private GameObject inventoryGrid;
    private GameObject template;
    public InventoryUiController(
        int defaultNumberOfRow, 
        GameObject defaultRow,
        GameObject inventoryGrid,
        GameObject template
        )
    {
        this.defaultNumberOfRow = defaultNumberOfRow;
        this.defaultRow = defaultRow;
        this.inventoryGrid = inventoryGrid;
        this.template = template;
    }

    public void SetupGrid()
    {
        for (int i = 0; i < defaultNumberOfRow; i++)
        {
            GameObject row = GameObject.Instantiate(defaultRow);
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
        ToggleUi();
    }

    public void UpdateSlotUi(int index, InventorySlot slot)
    {
        GameObject slotUi = GetInventorySlotUiByIndex(index);

        foreach (Transform child in slotUi.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        GameObject slotContent = GameObject.Instantiate(template);
        slotContent.transform.Find("Icon").GetComponent<Image>().sprite = slot.item.droppedItem.GetComponent<SpriteRenderer>().sprite;
        slotContent.GetComponent<ItemInteractionHandler>().collectibleItem = slot.item;
        slotContent.transform.Find("Count").GetComponent<TMP_Text>().text = slot.count.ToString();
        slotContent.transform.SetParent(slotUi.transform);
        slotContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
    }

    public void ToggleUi()
    {
        bool isActive = inventoryGrid.transform.GetChild(1).gameObject.activeSelf;
        for (int i = 1; i < inventoryGrid.transform.childCount; i++)
        {
            inventoryGrid.transform.GetChild(i).gameObject.SetActive(!isActive);
        }

        PlayerStatusRepository.SetIsViewingUi(!isActive);
    }

    private GameObject GetInventorySlotUiByIndex(int index)
    {
        return inventoryGrid.transform.GetChild(index / 10).GetChild(index % 10).gameObject;
    }
}
