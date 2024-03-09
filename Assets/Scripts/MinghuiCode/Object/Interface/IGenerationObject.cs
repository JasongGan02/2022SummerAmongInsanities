using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGenerationObject
{
    GameObject[] Prefabs { get;}
    public bool NeedsBackground { get;}
    public bool IsLit { get; }
    public float LightIntensity { get; }
    public GameObject GetGeneratedGameObjects();
    public GameObject GetGeneratedWallGameObjects();

}
