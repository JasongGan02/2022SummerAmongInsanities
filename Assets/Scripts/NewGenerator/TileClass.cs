using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName ="newtileclass", menuName = "Tile Class")]
public class TileClass : ScriptableObject
{
    public string tileName;
    //public Sprite tileSprite;
    public Sprite[] tileSprites;
}
