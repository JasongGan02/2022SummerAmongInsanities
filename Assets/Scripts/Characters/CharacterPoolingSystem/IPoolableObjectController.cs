using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolableObjectController
{
    void Reinitialize();

    public BaseObject PoolableObject
    {
        get;
    }
}
