using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "TerrainSettings", menuName = "Resources/TerrainSettings")]
public class TerrainSettings : ScriptableObject
{
    [field: Tooltip("Mountain height multiplier (the higher this is, steeper the mountains)")]
    [field: SerializeField] public TileAtlas tileAtlas { get; private set; }
    [field: Tooltip("Mountain height multiplier (the higher this is, steeper the mountains)")]
    [field: SerializeField] public float heightMultiplier { get; private set; }
    [field: Tooltip("Addition to the overall height of the world, just makes the world taller")]
    [field: SerializeField] public int heightAddition { get; private set; }
    [field: Tooltip("Number of dirt tiles between grass tile on the terrain surface, and the stone layer")]
    [field: SerializeField] public int dirtLayerHeight { get; private set; }
    [field: Tooltip("The frequency of the perlin noise for generating terrain mountains")]
    [field: Range(0, 1)][field: SerializeField] public float terrainFrequency { get; private set; }
    [field: Tooltip("The frequency of the perlin noise for generating terrain caves")]
    [field: Range(0, 1)][field: SerializeField] public float caveFrequency { get; private set; }
    [field: Tooltip("Percent chance for tall grass to spawn")]
    [field: Range(0, 100)][field: SerializeField] public int tallGrassChance { get; private set; }
    [field: Tooltip("Percent chance for trees to spawn")]
    [field: Range(0, 100)][field: SerializeField] public int treeChance { get; private set; }
    [field: Tooltip("Percent chance for vines to spawn")]
    [field: Range(0, 100)][field: SerializeField] public int vineChance { get; private set; }
    [field: Tooltip("Percent chance for stalactites (in caves) to spawn")]
    [field: Range(0, 100)][field: SerializeField] public int stalactiteChance { get; private set; }
    [field: Tooltip("Percent chance for pots or treasure to spawn")]
    [field: Range(0, 100)][field: SerializeField] public int potChance { get; private set; }
    [field: Tooltip("Define all the ores for the world here")]
    [field: SerializeField] public OreClass[] ores { get; private set; }

}
