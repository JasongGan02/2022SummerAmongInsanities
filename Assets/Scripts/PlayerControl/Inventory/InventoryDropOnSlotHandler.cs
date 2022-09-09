using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropOnSlotHandler : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    private GameObject player;
    private Inventory inventory;

    private void Start()
    {
        player = GameObject.Find(Constants.Name.PLAYER);
        inventory = player.GetComponent<Inventory>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("ON DROP");
        if (eventData.pointerDrag != null)
        {
            eventData.pointerDrag.transform.SetParent(gameObject.transform);
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            ItemInteractionHandler handler = eventData.pointerDrag.GetComponent<ItemInteractionHandler>();
            handler.OnMovedToAnotherSlot(GetSlotIndex(gameObject.name));
            handler.OnEndDrag(eventData);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                
                Debug.Log("left click on slot: " + gameObject.name);
                break;
            case PointerEventData.InputButton.Right:
                Debug.Log("right click on slot: " + gameObject.name);
                inventory.OnSlotClicked(GetSlotIndex(gameObject.name), Input.GetKey(KeyCode.LeftShift));
                break;
        }
    }

    private int GetSlotIndex(string name)
    {
        return int.Parse(name.Remove(0, 4));
    }
}
