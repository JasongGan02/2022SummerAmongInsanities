using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileAtlas", menuName = "Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    [Header("Enviroment")]
    public TileObject dirt;
    public TileObject stone;
    public TileObject grass;
    public TileObject natureAddons;
    public TileObject tree;

    [Header("Ores")]
    public TileObject coal;
    public TileObject iron;
    public TileObject gold;
   
}
