using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpawnable
{
    
    public float MinLightLevel { get; }
    public float MaxLightLevel { get; }
    public float SpawnWeight  { get; }
    public int PackSize  { get; }
    public string Biome { get; }
    public Vector2Int ColliderSize { get; }
    
}
 