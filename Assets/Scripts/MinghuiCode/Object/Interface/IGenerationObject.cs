using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGenerationObject
{
    GameObject[] Prefabs { get; set; }

    public GameObject GetGeneratedGameObjects();
}