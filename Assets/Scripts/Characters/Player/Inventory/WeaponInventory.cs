using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory : BaseInventory
{

    protected override void Awake()
    {
        inventoryGrid = GameObject.Find(Constants.Name.WEAPONINVENTORY_GRID);
        database = new InventoryDatabase(1, 0); 
        uiController = new InventoryUiController(
            1,
            defaultRow, 
            inventoryGrid,
            template,
            FindObjectOfType<UIViewStateManager>()
        );
        uiController.SetupWeaponUi();
    }

   


}
