using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "zombie", menuName = "Zombie")]
public class ZombieSO : ScriptableObject
{
    public GameObject prefab;

    private float movingSpeed = 1;
    private float DashRange = 5;

}
