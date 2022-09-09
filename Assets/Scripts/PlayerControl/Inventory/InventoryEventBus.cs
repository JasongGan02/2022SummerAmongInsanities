using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryEventBus
{
    public event EventHandler<OnSlotClickedEventArgs> OnSlotClickedEvent;
    public class OnSlotClickedEventArgs : EventArgs
    {
        public int slotIndex;
        public bool isShiftDown;
    }

    public void OnSlotClicked(int index, bool isShiftDown)
    {
        OnSlotClickedEvent?.Invoke(this, new OnSlotClickedEventArgs { slotIndex = index, isShiftDown = isShiftDown }) ;
    }
}
