using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;

public class ItemInteractionHandler : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    [HideInInspector]
    public IInventoryObject item;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private GameObject canvas;

    
    public Transform originalParent { get; private set; }
    public BaseInventory originatingInventory { get; private set; }

    private bool isInSlot = true;
    // no need to update currentSlotIndex, because when the item is moved to another slot
    // the slot template gets recreated. So the index is updated automatically onStart
    public int currentSlotIndex;
    private bool shouldListenForRightClick = false;

    private void Start()
    {
      

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GameObject.Find(Constants.Name.CANVAS);

        currentSlotIndex = GetSlotIndex(gameObject.transform.parent.name);
    }

    private void Update()
    {

    }
    private void HandleSlotRightClickEvent(object sender, InventoryEventBus.OnSlotRightClickedEventArgs args)
    {
        if (shouldListenForRightClick)
        {
            if (args.isShiftDown)
            {
                int currentCount = originatingInventory.GetInventorySlotAtIndex(currentSlotIndex).count;
                PartiallyMoveToAnotherSlot(originatingInventory, args.slotIndex, currentCount == 1 ? 1 : currentCount / 2, false);
            }
            else
            {
                PartiallyMoveToAnotherSlot(originatingInventory, args.slotIndex, 1, false);
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
        originatingInventory.RemoveSlotRightClickedHandler(HandleSlotRightClickEvent);
        Invoke(nameof(RemoveItemFromInventoryIfNotInSlot), 0.01f);
    }

    private void RemoveItemFromInventoryIfNotInSlot()
    {
        if (!isInSlot)
        {
            originatingInventory.RemoveItemAndDrop(currentSlotIndex);
            Destroy(gameObject);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originatingInventory = FindDirectInventory(transform, canvas.transform);

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        PlayerStatusRepository.SetIsViewingUi(true);
        isInSlot = false;
        shouldListenForRightClick = true;
        gameObject.transform.SetParent(canvas.transform);
        originatingInventory.AddSlotRightClickedHandler(HandleSlotRightClickEvent);
    }

    public void OnMovedToAnotherSlot(int anotherSlotIndex)
    {
        isInSlot = true;

        
            InventorySlot fromSlot = originatingInventory.GetInventorySlotAtIndex(currentSlotIndex);
            InventorySlot toSlot = originatingInventory.GetInventorySlotAtIndex(anotherSlotIndex);
            if (toSlot.IsEmpty || fromSlot.item.GetItemName() == toSlot.item.GetItemName())
            {
                PartiallyMoveToAnotherSlot(originatingInventory, anotherSlotIndex, fromSlot.count, true);
            }
            else
            {
            originatingInventory.SwapItems(currentSlotIndex, anotherSlotIndex);
            }
       
    }

    public void OnMovedToAntherInventorySlot(Transform dropTargetInventory,int anotherSlotIndex)
    {

        isInSlot = true;

        BaseInventory ToInventory = dropTargetInventory.GetComponentInChildren<BaseInventory>();
        originatingInventory.SwapItemsBetweenInventory(ToInventory, currentSlotIndex, anotherSlotIndex);

    }



    private void PartiallyMoveToAnotherSlot(BaseInventory invenotry,int anotherSlotIndex, int amount, bool shouldUpdateCurrentIndex)
    {
        int remainingItemCount = invenotry.MoveItems(currentSlotIndex, anotherSlotIndex, amount, shouldUpdateCurrentIndex);
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



    private BaseInventory FindDirectInventory(Transform child, Transform parent)
    {
        Transform current = child;
        // Climb up the hierarchy until we find the direct child of the specified parent
        while (current.parent != null && current.parent != parent)
        {
            current = current.parent;
        }
        
        return current.GetComponentInChildren<BaseInventory>();
    }
}
