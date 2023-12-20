using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpawnManager : MonoBehaviour
{
    public EquipmentObject[] equipments;
    public TowerObject[] testCase; //to-do just a test. do a list
    public DivinityFragObject divinityFrag;
    public MedicineObject[] medicineObjects;
    public TorchObject[] torchObjects;
    [SerializeField]
    private GameObject spawnPoint;

    void Start()
    {
        foreach (TowerObject each in testCase)
        {
            GameObject dropTower = each.GetDroppedGameObject(1);
            dropTower.transform.position = spawnPoint.transform.position;
        }
        SpawnFrags(30);
    }
    // only a temporary solution
    public void SpawnRamdonWeapon() 
    {
        spawnPoint.transform.position = FindObjectOfType<CoreArchitecture>().transform.position;
        if (equipments.Length == 0) return;

        int index = Random.Range(0, equipments.Length);
        GameObject drop = equipments[index].GetDroppedGameObject(1);
        drop.transform.position = spawnPoint.transform.position;

        GameObject dropTower = testCase[0].GetDroppedGameObject(1);
        dropTower.transform.position = spawnPoint.transform.position;

        foreach (TowerObject each in testCase)
        {
            dropTower = each.GetDroppedGameObject(1);
            dropTower.transform.position = spawnPoint.transform.position;
        }
        spawnPoint.transform.position = FindObjectOfType<CoreArchitecture>().transform.position;
    }

    public void SpawnFrags(int num) 
    {
        spawnPoint.transform.position = FindObjectOfType<CoreArchitecture>().transform.position;
        GameObject drop = divinityFrag.GetDroppedGameObject(num);
        drop.transform.position = spawnPoint.transform.position;
    }

    public void SpawnMedicine()
    {
        foreach(MedicineObject each in medicineObjects)
        {
            GameObject m1 = each.GetDroppedGameObject(1);
            m1.transform.position = spawnPoint.transform.position;
        }
    }

    public void SpawnTorch()
    {
        foreach(TorchObject each in torchObjects)
        {
            GameObject p = each.GetDroppedGameObject(1);
            p.transform.position = spawnPoint.transform.position;
        }
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
        if (GUILayout.Button("Spawn Medicine"))
        {
            spawnManager.SpawnMedicine();
        }
        if (GUILayout.Button("Spawn Torch"))
        {
            spawnManager.SpawnTorch();
        }
    }
}