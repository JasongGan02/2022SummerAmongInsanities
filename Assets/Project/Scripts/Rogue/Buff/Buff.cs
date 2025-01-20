using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
//[CreateAssetMenu(menuName = "Effects")]
public class Buff //: ScriptableObject
{
    public string name;
    public int level;
    public Action effect;
    public bool canRepeat;

    public Buff() { }

    public Buff (string name)
    {
        this.name = name;
    }
}
