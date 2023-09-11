using System.Collections;
using UnityEngine;
[System.Serializable]
public class BiomeClass
{
    public string biomeName;
    //public Color biomeColor;
    public TileAtlas biomeTiles; //rewritten the corresponding tile in original tile;

    [Header("Noise Settings")]
    public float terrainFreq = 0.05f;
    public float caveFreq = 0.05f;
    public Texture2D caveNoiseTexture;

    [Header("Generation Settings")]
    public bool generateCave = true;
    public int dirtLayerHeight = 5;
    public float surfacePortion = 0.25f;
    public float heightMultiplier = 15f;

    [Header("Nature Addons")]
    public int addonsChance;
    public int treeChance = 10;

    [Header("Ore Settings")]
    public OreClass[] ores;
}
