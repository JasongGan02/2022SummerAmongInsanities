using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryEventBus
{
    public event EventHandler<OnSlotRightClickedEventArgs> OnSlotRightClickedEvent;
    public class OnSlotRightClickedEventArgs : EventArgs
    {
        public int slotIndex;
        public bool isShiftDown;
    }

    public event EventHandler<OnSlotLeftClickedEventArgs> OnSlotLeftClickedEvent;
    public class OnSlotLeftClickedEventArgs : EventArgs
    {
        public int slotIndex;
    }

    public void OnSlotRightClicked(int index, bool isShiftDown)
    {
        OnSlotRightClickedEvent?.Invoke(this, new OnSlotRightClickedEventArgs { slotIndex = index, isShiftDown = isShiftDown }) ;
    }

    public void OnSlotLeftClicked(int index)
    {
        OnSlotLeftClickedEvent?.Invoke(this, new OnSlotLeftClickedEventArgs { slotIndex = index });
    }
}
