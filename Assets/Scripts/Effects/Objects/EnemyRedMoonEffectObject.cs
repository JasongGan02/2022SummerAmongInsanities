using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Stats/EnemyRedMoonEffect")]
public class EnemyRedMoonEffectObject : StatsEffectObject
{
    public override void ExecuteEffect(IEffectableController effectedGameController)
    {
        duration = FindObjectOfType<TimeSystemManager>().dayToRealTimeInSecond / 2f;
        base.ExecuteEffect(effectedGameController);
    }
}
