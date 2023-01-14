using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpawnManager : MonoBehaviour
{
    public EquipmentObject[] equipments;
    
    [SerializeField]
    private GameObject spawnPoint;

    // only a temporary solution
    public void SpwanRamdonWeapon() 
    {
        if (equipments.Length == 0) return;

        int index = Random.Range(0, equipments.Length);
        GameObject drop = equipments[index].GetDroppedGameObject(1);
        drop.transform.position = spawnPoint.transform.position;
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