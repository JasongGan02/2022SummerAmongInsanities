using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory : BaseInventory
{
    private Dictionary<int, GameObject> spawnedWeapons = new Dictionary<int, GameObject>();




    protected override void Update()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");



    }

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

    public override void SpawnWeaponIfAvailable()
    {
        // Clear out any invalid entries from previous runs
        foreach (var weapon in spawnedWeapons.Values)
        {
            if (weapon != null)
            {
                Destroy(weapon);
            }
        }
        spawnedWeapons.Clear();

        // Spawn weapons for each slot in WeaponInventory
        for (int i = 0; i < database.GetSize(); i++)
        {
            InventorySlot slot = database.GetInventorySlotAtIndex(i);

            if (slot != null && slot.item is WeaponObject weapon)
            {
                // Spawn weapon and track it by slot index
                GameObject spawnedWeapon = weapon.GetSpawnedGameObject(player.GetComponent<PlayerController>());
                spawnedWeapons[i] = spawnedWeapon; // Add to dictionary with slot index as key
            }

            // Stop if we have reached the maximum of two weapons
            if (spawnedWeapons.Count >= 2)
            {
                break;
            }
        }
    }

    public override void DestroySpawnedWeapon(int slotIndex)
    {
        if (spawnedWeapons.ContainsKey(slotIndex) && spawnedWeapons[slotIndex] != null)
        {
            Destroy(spawnedWeapons[slotIndex]);
            spawnedWeapons.Remove(slotIndex); // Remove the entry after destruction
        }
    }




}


