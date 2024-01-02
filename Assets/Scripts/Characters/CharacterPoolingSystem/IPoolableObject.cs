using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolableObject
{
    public GameObject GetPoolGameObject();
}