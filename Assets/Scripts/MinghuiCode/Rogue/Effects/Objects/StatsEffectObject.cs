using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(menuName = "Effects/Stats")]
public class StatsEffectObject : EffectObject 
{
    [Header ("Stats Effect Object")]
    public float dHP;
    
    public override void ExecuteEffect(IEffectableObject effectedGameObject) //Use this when you are unsure about what type of controller will be using.
    {
        string controllerName = "StatsEffectController";
        Type type = Type.GetType(controllerName);
        var controller = (effectedGameObject as MonoBehaviour).gameObject.AddComponent(type);
        (controller as EffectController).Initialize(this);
    }
}
