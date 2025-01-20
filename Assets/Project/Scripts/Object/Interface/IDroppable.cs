using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDroppable 
{
    public GameObject GetDroppedGameObject(int amount, Vector3 dropPosition);
}
