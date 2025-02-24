using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    [Header("General Settings")]
    [Tooltip("When true, debug functions (like spawning equipment or towers) can be invoked via buttons.")]
    public bool isDebug = true;

    [Header("Equipment Settings")]
    public EquipmentObject[] equipments;

    [Header("Tower Settings")]
    public TowerObject[] towerObjects;

    [Header("Ash Settings")]
    public AshObject ash;

    [Header("Initial Inventory Items")]
    [Tooltip("These items are spawned automatically when not in debug mode.")]
    public SpawnedObject[] initialItems;

    private Inventory inventory;
    private Vector3 coreSpawnPosition;

    private void Start()
    {
        // Locate the Inventory in the scene.
        inventory = FindObjectOfType<Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning("Inventory not found in the scene.");
        }

        // Begin waiting for CoreArchitectureController to be available.
        StartCoroutine(WaitForCoreArchitectureAndSpawn());
    }

    /// <summary>
    /// Waits until the CoreArchitectureController instance is available,
    /// then sets the core spawn position and, if not in debug mode, spawns initial items.
    /// </summary>
    private IEnumerator WaitForCoreArchitectureAndSpawn()
    {
        // Wait until the CoreArchitectureController instance is available.
        yield return new WaitUntil(() => CoreArchitectureController.Instance != null);

        // Find the SpawnPoint under the CoreArchitectureController.
        Transform spawnPoint = CoreArchitectureController.Instance.transform.Find("SpawnPoint");
        if (spawnPoint != null)
        {
            coreSpawnPosition = spawnPoint.position;
            Debug.Log($"Core spawn position set to: {coreSpawnPosition}");
        }
        else
        {
            Debug.LogWarning("SpawnPoint not found under CoreArchitectureController.");
        }

        // If not in debug mode, automatically spawn initial inventory items.
        if (!isDebug)
        {
            SpawnInitialItems();
        }
    }

    /// <summary>
    /// Spawns a random equipment item from the equipments array at the core spawn position.
    /// </summary>
    public void SpawnEquipment()
    {
        if (equipments == null || equipments.Length == 0)
        {
            Debug.LogWarning("No equipment available to spawn.");
            return;
        }

        int index = Random.Range(0, equipments.Length);
        GameObject equipmentObj = equipments[index].GetDroppedGameObject(1, coreSpawnPosition);
        Debug.Log($"Spawned equipment: {equipmentObj.name} at {coreSpawnPosition}");
    }
    
    /// <summary>
    /// Spawns the ash object at the core spawn position.
    /// </summary>
    public void SpawnAsh()
    {
        if (ash == null)
        {
            Debug.LogWarning("Ash object is not assigned in the SpawnManager.");
            return;
        }

        if (coreSpawnPosition == Vector3.zero)
        {
            Debug.LogWarning("Core spawn position is not set.");
            return;
        }

        GameObject ashObj = ash.GetDroppedGameObject(1, coreSpawnPosition);
        Debug.Log($"Spawned ash: {ashObj.name} at {coreSpawnPosition}");
    }

    /// <summary>
    /// Spawns the initial inventory items from the initialItems array into the Inventory.
    /// </summary>
    public void SpawnInitialItems()
    {
        if (inventory == null)
        {
            Debug.LogWarning("Inventory not found. Cannot spawn initial items.");
            return;
        }

        if (initialItems == null || initialItems.Length == 0)
        {
            Debug.LogWarning("No initial items assigned.");
            return;
        }

        foreach (SpawnedObject item in initialItems)
        {
            if (item.inventoryObject is IInventoryObject inventoryItem)
            {
                inventory.AddItem(inventoryItem, item.number);
                Debug.Log($"Added initial item: {item.inventoryObject.name} x{item.number}");
            }
            else
            {
                Debug.LogWarning("An initial item is not of type IInventoryObject.");
            }
        }
    }

    /// <summary>
    /// Debug method that spawns a random weapon by spawning equipment and towers.
    /// </summary>
    public void SpawnRandomWeapon()
    {
        SpawnEquipment();
    }

    /// <summary>
    /// Debug method to spawn the ash object.
    /// </summary>
    public void SpawnDebugAsh()
    {
        SpawnAsh();
    }
}

[Serializable]
public class SpawnedObject
{
    public BaseObject inventoryObject;
    public int number;
}