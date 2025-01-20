using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropOnSlotHandler : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    
    private Inventory inventory;

    private void Start()
    {
        inventory =  FindObjectOfType<Inventory>();
    }


    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        { 

            Canvas canvas = FindObjectOfType<Canvas>();
            ItemInteractionHandler handler = eventData.pointerDrag.GetComponent<ItemInteractionHandler>();
            
            Transform draggedItemInventory = FindDirectChildOfParent(handler.originalParent, canvas.transform);
            Transform dropTargetInventory = FindDirectChildOfParent(gameObject.transform, canvas.transform);

           
            bool isSameInventory = draggedItemInventory == dropTargetInventory;
            eventData.pointerDrag.transform.SetParent(gameObject.transform);
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            if (isSameInventory)
            {

                handler.OnMovedToAnotherSlot(GetSlotIndex(gameObject.name));
              
            }
            else
            {

                handler.OnMovedToAntherInventorySlot(dropTargetInventory, GetSlotIndex(gameObject.name));
         

            }
            handler.OnEndDrag(eventData);

        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        int slotIndex = GetSlotIndex(gameObject.name);

        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                if (inventory.GetInventorySlotAtIndex(slotIndex).item == null) return;
                Debug.Log("left click on slot: " + gameObject.name);
                inventory.OnSlotLeftClicked(slotIndex);
                break;
            case PointerEventData.InputButton.Right:
                Debug.Log("right click on slot: " + gameObject.name);
                inventory.OnSlotRightClicked(slotIndex, Input.GetKey(KeyCode.LeftShift));
                break;
        }
    }

    private int GetSlotIndex(string name)
    {
        return int.Parse(name.Remove(0, 4));
    }

    private Transform FindDirectChildOfParent(Transform child, Transform parent)
    {
        Transform current = child;
        // Climb up the hierarchy until we find the direct child of the specified parent
        while (current.parent != null && current.parent != parent)
        {
            current = current.parent;
        }   
        return current;
    }

}
