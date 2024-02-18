using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    public InventoryDatabase ChestInventory { get; private set; }

    void Awake()
    {
        // Initialize the inventory for this chest
        ChestInventory = new InventoryDatabase(defaultNumberOfRow: 4, maxExtraRow: 0);
        // Load existing items into the inventory if needed
    }

    public void OpenChest()
    {
        Debug.Log("chest open");
    }

}

