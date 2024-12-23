using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    public EquipmentObject[] equipments;
    public TowerObject[] testCase; //to-do just a test. do a list
    public DivinityFragObject divinityFrag;
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

    public void SpawnFrags(int num) 
    {
        GameObject drop = divinityFrag.GetDroppedGameObject(num, coreSpawnPosition);
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
        SpawnFrags(30);

    }
}

[CustomEditor(typeof(SpawnManager))]
public class SpawnManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpawnManager spawnManager = (SpawnManager)target;
        if (GUILayout.Button("Spawn ramdon weapon"))
        {
            spawnManager.SpawnRamdonWeapon();
        }
        if (GUILayout.Button("Spawn 10 Frags"))
        {
            spawnManager.SpawnFrags(10);
        }
    }
}

[Serializable]
public class SpawnedObject
{
    public BaseObject inventoryObject;
    public int number;
}