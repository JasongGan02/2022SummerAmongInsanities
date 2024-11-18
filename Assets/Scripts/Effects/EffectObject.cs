using System;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Effects")]
public abstract class EffectObject : ScriptableObject
{
    [Header("General Effect Settings")]
    public float duration;
    public bool requiresReset = false;
    public bool isStackable = false;
    public bool isReselectable = true;
    public bool isPermanent = false;
    public string description; 
    public MonoScript componentToApply;
    public MonoScript effectControllerType;
    //TODO: Sprite 2DIcon, Potential VFX
    
    public virtual void ExecuteEffect(IEffectableController effectedGameController)
    {
        Type controllerType = effectControllerType?.GetClass();
        if (controllerType == null)
        {
            Debug.LogError("Effect controller type not set or invalid.");
            return;
        }

        MonoBehaviour monoBehaviour = effectedGameController as MonoBehaviour;
        if (monoBehaviour == null)
        {
            Debug.LogError("Effected game object is not set or invalid.");
            return;
        }

        EffectController controller = monoBehaviour.gameObject.AddComponent(controllerType) as EffectController;
        if (controller == null)
        {
            Debug.LogError("Failed to add controller component.");
            return;
        }

        controller.Initialize(this);
    }

    public Type GetComponentToApply()
    {
        return componentToApply?.GetClass();
    }
    
    public Type GetEffectComponent()
    {
        return effectControllerType?.GetClass();
    }
}
