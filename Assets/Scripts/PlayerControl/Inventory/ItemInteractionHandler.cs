using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ItemInteractionHandler : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    [HideInInspector]
    public IInventoryObject item;
    private GameObject player;
    private Inventory inventory;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private GameObject canvas;

    private bool isInSlot = true;
    // no need to update currentSlotIndex, because when the item is moved to another slot
    // the slot template gets recreated. So the index is updated automatically onStart
    private int currentSlotIndex;
    private bool shouldListenForRightClick = false;

    private void Start()
    {
        player = GameObject.Find(Constants.Name.PLAYER);
        inventory = player.GetComponent<Inventory>();

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GameObject.Find(Constants.Name.CANVAS);

        currentSlotIndex = GetSlotIndex(gameObject.transform.parent.name);
    }

    private void HandleSlotRightClickEvent(object sender, InventoryEventBus.OnSlotRightClickedEventArgs args)
    {
        if (shouldListenForRightClick)
        {
            if (args.isShiftDown)
            {
                int currentCount = inventory.GetInventorySlotAtIndex(currentSlotIndex).count;
                PartiallyMoveToAnotherSlot(args.slotIndex, currentCount == 1 ? 1 : currentCount / 2, false);
            }
            else
            {
                PartiallyMoveToAnotherSlot(args.slotIndex, 1, false);
            }
        }
    }

    private int GetSlotIndex(string name)
    {
        return int.Parse(name.Remove(0, 4));
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        shouldListenForRightClick = false;
        PlayerStatusRepository.SetIsViewingUi(false);
        inventory.RemoveSlotRightClickedHandler(HandleSlotRightClickEvent);
        Invoke("RemoveItemFromInventoryIfNotInSlot", 0.01f);
    }

    private void RemoveItemFromInventoryIfNotInSlot()
    {
        if (!isInSlot)
        {
            inventory.RemoveItemAndDrop(currentSlotIndex);
            Destroy(gameObject);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        PlayerStatusRepository.SetIsViewingUi(true);
        isInSlot = false;
        shouldListenForRightClick = true;
        gameObject.transform.SetParent(canvas.transform);
        inventory.AddSlotRightClickedHandler(HandleSlotRightClickEvent);
    }

    public void OnMovedToAnotherSlot(int anotherSlotIndex)
    {
        isInSlot = true;

        InventorySlot fromSlot = inventory.GetInventorySlotAtIndex(currentSlotIndex);
        InventorySlot toSlot = inventory.GetInventorySlotAtIndex(anotherSlotIndex);
        if (toSlot.IsEmpty || fromSlot.item.GetItemName() == toSlot.item.GetItemName())
        {
            PartiallyMoveToAnotherSlot(anotherSlotIndex, fromSlot.count, true);
        }
        else
        {
            inventory.SwapItems(currentSlotIndex, anotherSlotIndex);
        }
    }

    private void PartiallyMoveToAnotherSlot(int anotherSlotIndex, int amount, bool shouldUpdateCurrentIndex)
    {
        int remainingItemCount = inventory.MoveItems(currentSlotIndex, anotherSlotIndex, amount, shouldUpdateCurrentIndex);
        if (remainingItemCount >= 0)
        {
            TMP_Text countText = gameObject.transform.Find("Count").GetComponent<TMP_Text>();
            countText.text = remainingItemCount.ToString();

            if (remainingItemCount == 0)
            {
                OnEndDrag(new PointerEventData(EventSystem.current));
                // no need to destroy the game object here since it will be handle is OnEndDrag
            }
        }
    }
}
