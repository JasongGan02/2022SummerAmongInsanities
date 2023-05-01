using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Effects")]
public class Effect : ScriptableObject
{
   // public string name;
    public int cost;
    public int level;
    public Action effect;
    public bool canRepeat;

}
