using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int deathCount;
    public Vector3 playerPosition;

    //the values defined in this constructor will be the default values 
    //the game starts with when there is no data to load
    public GameData()
    {
        this.deathCount = 0;
        this.playerPosition = Vector3.zero;
    }
}
