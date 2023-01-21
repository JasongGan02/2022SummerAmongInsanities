using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Buff
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
