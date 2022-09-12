using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ItemInteractionHandler : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    [HideInInspector]
    public CollectibleObject collectibleItem;
    private GameObject player;
    private Inventory inventory;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private GameObject canvas;

    private bool isInSlot = true;
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
        inventory.AddSlotLeftClickedHandler(HandleSlotLeftClickEvent);
    }

    private void OnDestroy()
    {
        inventory?.RemoveSlotLeftClickedHandler(HandleSlotLeftClickEvent);
    }

    private void HandleSlotRightClickEvent(object sender, InventoryEventBus.OnSlotRightClickedEventArgs args)
    {
        if (shouldListenForRightClick)
        {
            Debug.Log("handling slot click. Slot index = " + args.slotIndex);
            Debug.Log("The dragged item = " + collectibleItem.name);
            if (args.isShiftDown)
            {
                int currentCount = inventory.GetInventorySlotAtIndex(currentSlotIndex).count;
                PartiallyMoveToAnotherSlot(args.slotIndex, currentCount == 1 ? 1 : currentCount / 2);
            }
            else
            {
                PartiallyMoveToAnotherSlot(args.slotIndex, 1);
            }
        }
    }

    public void HandleSlotLeftClickEvent(object sender, InventoryEventBus.OnSlotLeftClickedEventArgs args)
    {
        Debug.Log(args.slotIndex);
        if (args.slotIndex == currentSlotIndex)
        {
            Debug.Log("using " + collectibleItem.name);
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
        Debug.Log("ON END DRAG");
        inventory.RemoveSlotRightClickedHandler(HandleSlotRightClickEvent);
        Invoke("RemoveItemFromInventoryIfNotInSlot", 0.01f);
    }

    private void RemoveItemFromInventoryIfNotInSlot()
    {
        if (!isInSlot)
        {
            inventory.RemoveItem(currentSlotIndex);
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
        inventory.SwapItems(currentSlotIndex, anotherSlotIndex);
        currentSlotIndex = anotherSlotIndex;
    }

    private void PartiallyMoveToAnotherSlot(int anotherSlotIndex, int amount)
    {
        if (inventory.MoveItems(currentSlotIndex, anotherSlotIndex, amount))
        {
            TMP_Text countText = gameObject.transform.Find("Count").GetComponent<TMP_Text>();
            countText.text = (int.Parse(countText.text) - amount).ToString();

            if (countText.text == "0")
            {
                OnEndDrag(new PointerEventData(EventSystem.current));
                Destroy(gameObject);
            }
        }
    }
}
