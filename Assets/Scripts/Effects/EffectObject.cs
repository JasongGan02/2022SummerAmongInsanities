using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Effects")]
public abstract class EffectObject : ScriptableObject
{
    [Header("General Effect Settings")]
    public float duration;
    public float tickDuration = 0f;
    public bool requiresReset = false;
    public bool isStackable = false;
    public bool isPermanent = false;
    public string description; 
    public MonoScript componentToApply;
    public MonoScript effectControllerType;
    //TODO: Sprite 2DIcon, Potential VFX
    [Header("Visual Effect Settings")]
    public List<VFX> vfxList = new List<VFX>();

    
    public virtual void ExecuteEffect(IEffectableController effectedGameController) //apply effect on a single object
    {
        // Get the type of the EffectController to be applied
        Type controllerType = effectControllerType?.GetClass();
        if (controllerType == null)
        {
            Debug.LogError("Effect controller type not set or invalid.");
            return;
        }

        // Add the EffectController component to the target game object
        EffectController effectController = (effectedGameController as MonoBehaviour).gameObject.AddComponent(controllerType) as EffectController;

        if (effectController != null)
        {
            // Initialize the EffectController with this EffectObject
            effectController.Initialize(this);
        }
        else
        {
            Debug.LogError("Failed to add the effect controller component.");
        }
    }

    
    public virtual void InitializeEffectObject()
    {
        EffectEvents.ApplyEffect(this, GetComponentToApply());
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
