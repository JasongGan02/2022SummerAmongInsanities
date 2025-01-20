using System;
using UnityEngine;

[Serializable]
public class VFX
{
    public string name; // Name or description of the VFX
    public GameObject vfxPrefab; // Prefab of the particle effect
    public bool attachAtStart = true; // Whether to start at the beginning
    public bool attachToTarget = true; // Whether to parent the VFX to the target
}