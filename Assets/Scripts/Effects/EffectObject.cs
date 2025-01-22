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
    [Header("Select the effect controller type")]
    // The .txt file that contains the assembly-qualified name (or short name) of the effect controller
    [SerializeField] private TextAsset effectControllerType;

    [Header("Select some other component that you want this effect to be attached")]
    // Another .txt file that contains the type name of a component to apply
    [SerializeField] private TextAsset componentToApplyType;
    // -------------------------------------------------------------------
    // PROPERTIES that wrap the logic of reading from the .txt to a System.Type
    // -------------------------------------------------------------------
    public virtual Type EffectControllerType => ResolveTypeFromText(effectControllerType);
    public virtual Type ComponentToApplyType => ResolveTypeFromText(componentToApplyType);

    //TODO: Sprite 2DIcon, Potential VFX
    [Header("Visual Effect Settings")]
    public List<VFX> vfxList = new List<VFX>();

    
    public virtual void ExecuteEffect(IEffectableController effectedGameController)
    {
        Type controllerType = EffectControllerType;
        if (controllerType == null)
        {
            Debug.LogError("Effect controller type not set or invalid.");
            return;
        }

        var monoObj = effectedGameController as MonoBehaviour;
        if (monoObj == null)
        {
            Debug.LogError("Effected object is not a MonoBehaviour. Can't add component.");
            return;
        }

        EffectController effectController = 
            monoObj.gameObject.AddComponent(controllerType) as EffectController;

        if (effectController != null)
        {
            effectController.Initialize(this);
        }
        else
        {
            Debug.LogError($"Failed to add effect controller {controllerType.FullName}.");
        }
    }

    public virtual void InitializeEffectObject()
    {
        Type comp = ComponentToApplyType;
        if (comp == null)
        {
            Debug.LogWarning("No componentToApply was chosen.");
            return;
        }
        // Example usage
        EffectEvents.ApplyEffect(this, comp);
    }
    
    // -------------------------------------------------------------------
    // PRIVATE method that converts a TextAsset into a System.Type
    // -------------------------------------------------------------------
    private Type ResolveTypeFromText(TextAsset textAsset)
    {
        if (textAsset == null) return null;

        string rawTypeName = textAsset.name.Trim();
        if (string.IsNullOrEmpty(rawTypeName)) return null;
        // Attempt to get the type by the name stored in the .txt
        
        return Type.GetType(rawTypeName);
    }
}
