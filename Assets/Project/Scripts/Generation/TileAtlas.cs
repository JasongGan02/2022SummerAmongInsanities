using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileAtlas", menuName = "Atlas/Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    [Header("Environment")]
    public TileObject dirt;
    public TileObject stone;
    public TileObject grass;
    public TileObject natureAddons;
    public TileObject tree;

    [Header("Ores")]
    public TileObject coal;
    public TileObject iron;
    public TileObject gold;

    [Header("Special")] public TileObject block;

}
