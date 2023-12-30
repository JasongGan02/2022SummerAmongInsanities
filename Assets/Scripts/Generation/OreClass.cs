using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
[System.Serializable]
public class OreClass
{
    [field: Tooltip("The tile to place for this ore")]
    [field: SerializeField] public TileObject oreTile { get; private set; }
    [field: Tooltip("Highest Y position the tile can be found. Make sure to keep this below the world Y size")]
    [field: SerializeField] public int maxSpawnHeight { get; private set; }
    [field: Tooltip("Lowest Y position the tile can be found. Where 0 is the bottom of the world")]
    [field: SerializeField] public int minSpawnHeight { get; private set; }
    [field: Tooltip("Perlin noise frequency of this ore")]
    [field: SerializeField][field: Range(0, 1)] public float spawnFrequency { get; private set; }
    [field: Tooltip("Threshold of the perlin noise value for this ore to spawn. Make this higher to get larger clusters of ore")]
    [field: SerializeField][field: Range(0, 1)] public float spawnRadius { get; private set; }



}