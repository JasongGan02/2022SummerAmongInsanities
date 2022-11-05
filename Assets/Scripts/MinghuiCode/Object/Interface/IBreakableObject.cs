using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBreakableObject
{
    int HealthPoint { get; set; }

    Drop[] Drops { get; set; }

    public List<GameObject> GetDroppedGameObjects(bool isPlacedByPlayer);
}
