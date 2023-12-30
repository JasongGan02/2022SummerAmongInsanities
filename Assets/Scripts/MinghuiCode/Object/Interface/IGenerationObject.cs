using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGenerationObject
{
    GameObject[] Prefabs { get; set; }
    public bool NeedsBackground { get; set; }
    public bool IsLit { get; set; }
    public int TileID { get; set; }
    public GameObject GetGeneratedGameObjects();
    public GameObject GetGeneratedWallGameObjects();

}
