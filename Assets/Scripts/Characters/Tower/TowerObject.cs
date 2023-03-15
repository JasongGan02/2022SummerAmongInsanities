using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Character Objects/Tower Object")]
public class TowerObject : CharacterObject
{
    public float bullet_speed;
    public GameObject shadowPrefab;
    public GameObject bullet;
}
