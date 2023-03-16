using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerAtlas", menuName = "Atlas/Tower Atlas")]
public class TowerAtlas : ScriptableObject
{
    public TowerObject CatapultTower;
    public TowerObject ArcherTower;
    public TowerObject TrapTower;
    public TowerObject WoodWall;
    public TowerObject StoneWall;
}
