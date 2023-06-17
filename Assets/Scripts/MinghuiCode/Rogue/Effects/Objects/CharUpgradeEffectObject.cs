using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(menuName = "Effects/Upgrade")]
public class CharUpgradeEffectObject : EffectObject
{
    [Header ("Character Upgrade Effect Settings")]
    public float dHP;
    public float dAtkDamage;
    public float dAtkInterval;
    public float dAtkRange;
    public float dMovingSpeed;
    public float dJumpForce;

    public override void ExecuteEffect(IEffectableObject effectedGameObject) //Use this when you are unsure about what type of controller will be using.
    {
        string controllerName = "CharUpgradeEffectController";
        Type type = Type.GetType(controllerName);
        var controller = (effectedGameObject as MonoBehaviour).gameObject.AddComponent(type);
        (controller as EffectController).Initialize(this);
    }
}
