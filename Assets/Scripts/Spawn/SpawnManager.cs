using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    public EquipmentObject[] equipments;
    public TowerObject[] testCase; //to-do just a test. do a list
    [FormerlySerializedAs("divinityFrag")] public AshObject ash;
    public SpawnedObject[] inventoryObjects;
    private Inventory inventory;

    private Vector3 coreSpawnPosition;
    

    void Start()
    {
        StartCoroutine(WaitForCoreArchitectureAndDoSomething());
        
        inventory = FindObjectOfType<Inventory>();
        foreach (SpawnedObject spawnedObject in inventoryObjects)
        {
            inventory.AddItem(spawnedObject.inventoryObject as IInventoryObject, spawnedObject.number);
        }
    }
    // only a temporary solution
    public void SpawnRamdonWeapon() 
    {
        if (equipments.Length == 0) return;

        int index = Random.Range(0, equipments.Length);
        GameObject drop = equipments[index].GetDroppedGameObject(1, coreSpawnPosition);

        GameObject dropTower = testCase[0].GetDroppedGameObject(1, coreSpawnPosition);

        foreach (TowerObject each in testCase)
        {
            dropTower = each.GetDroppedGameObject(1, coreSpawnPosition);
        }
    }
    
    public void SpawnDebugInitialAsh()
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

        GameObject spawnedAsh = ash.GetDroppedGameObject(1, coreSpawnPosition);
        Debug.Log($"Spawned debug ash at {coreSpawnPosition}: {spawnedAsh.name}");
    }

    
    IEnumerator WaitForCoreArchitectureAndDoSomething()
    {
        // Wait until CoreArchitecture is available
        while (CoreArchitectureController.Instance == null)
        {
            yield return null; // Wait for the next frame
        }
        coreSpawnPosition = CoreArchitectureController.Instance.transform.Find("SpawnPoint").position;
        foreach (TowerObject each in testCase)
        {
            GameObject dropTower = each.GetDroppedGameObject(1, coreSpawnPosition);
        }

    }
}

[Serializable]
public class SpawnedObject
{
    public BaseObject inventoryObject;
    public int number;
}