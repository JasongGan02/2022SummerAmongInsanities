using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public EquipmentObject axe;
    public GameObject spawnPoint;
    private void Start()
    {
        InitializeAxes();
    }

    // only a temporary solution
    public void InitializeAxes() { 
    
        GameObject drop = axe.GetDroppedGameObject(1);
        drop.transform.position = spawnPoint.transform.position;
    }
}
