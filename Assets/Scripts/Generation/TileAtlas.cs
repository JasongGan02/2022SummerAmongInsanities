using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileAtlas", menuName = "Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    [Header("Enviroment")]
    public TileClass dirt;
    public TileClass stone;
    public TileClass grass;
    public TileClass natureAddons;
    public TileClass tree;

    [Header("Ores")]
    public TileClass coal;
    public TileClass iron;
    public TileClass gold;
   
}
