using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Stats/EnemyRedMoonEffect")]
public class EnemyRedMoonEffectObject : StatsEffectObject
{
    public override void InitializeEffectObject()
    {
        if (duration == 0)
            duration = TimeSystemManager.Instance.dayToRealTimeInSecond / 2f;
        Debug.Log($"Applied redMoon effect for {duration} seconds");
        base.InitializeEffectObject();
    }
}