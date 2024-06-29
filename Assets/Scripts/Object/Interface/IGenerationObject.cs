using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGenerationObject
{
    GameObject[] Prefabs { get;}
    public bool NeedsBackground { get;}
    public bool IsLit { get; }
    public float LightIntensity { get; }
    public GameObject GetGeneratedGameObjects(int spriteNumber, Quaternion rotation, bool flipX);
    public GameObject GetGeneratedWallGameObjects(int spriteNumber, Quaternion rotation, bool flipX);

}
