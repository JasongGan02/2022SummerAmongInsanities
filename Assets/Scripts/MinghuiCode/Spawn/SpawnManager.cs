using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpawnManager : MonoBehaviour
{
    public EquipmentObject[] equipments;
    public TowerObject[] testCase; //to-do just a test. do a list
    
    [SerializeField]
    private GameObject spawnPoint;

    void Start()
    {
        foreach(TowerObject each in testCase)
        {
            GameObject dropTower = each.GetDroppedGameObject(1);
            dropTower.transform.position = spawnPoint.transform.position;
        }
    }
    // only a temporary solution
    public void SpwanRamdonWeapon() 
    {
        if (equipments.Length == 0) return;

        int index = Random.Range(0, equipments.Length);
        GameObject drop = equipments[index].GetDroppedGameObject(1);
        drop.transform.position = spawnPoint.transform.position;

        GameObject dropTower = testCase[0].GetDroppedGameObject(1);
        dropTower.transform.position = spawnPoint.transform.position;
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
            spawnManager.SpwanRamdonWeapon();
        }
    }
}