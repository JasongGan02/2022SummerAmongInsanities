using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public EquipmentObject axe;
    private void Start()
    {
        InitializeAxes();
    }

    // only a temporary solution
    public void InitializeAxes()
    {
        var spawnObjects = GameObject.FindGameObjectsWithTag(Constants.Tag.SPAWN);

        foreach (var spawn in spawnObjects)
        {
            if (spawn.name == "Axe")
            {
                spawn.GetComponent<DroppedObjectController>().Initialize(axe, 1);
            }
        }
    }
}
