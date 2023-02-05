using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallTower : MonoBehaviour
{
    enum WallType{StoneWall, WoodenWall};
    [SerializeField]WallType wallType; 
    [SerializeField]int StoneWallLifePoint;
    [SerializeField]int WoodenWallLifePoint;
    // Start is called before the first frame update
    
    void DecreaseLifePoint(int damagePoint)
    {
        switch(wallType)
        {
            case WallType.StoneWall:
            StoneWallLifePoint -= damagePoint;
            if(StoneWallLifePoint <= 0)
            {
                Destroy(gameObject);
            }
            break;

            case WallType.WoodenWall:
            WoodenWallLifePoint -= damagePoint;
            if(WoodenWallLifePoint <= 0)
            {
                Destroy(gameObject);
            }
            break;
            
            default:
            Debug.LogWarning(gameObject.name + "'s WallType is not defined");
            break;
        }
    }

}
