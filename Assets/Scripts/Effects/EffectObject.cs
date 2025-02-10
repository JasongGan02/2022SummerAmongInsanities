using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

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
    // The .txt file that contains the assembly-qualified name (or short name) of the effect controller.
    [SerializeField] private TextAsset effectControllerType;

    [Header("Select some other component that you want this effect to be attached")]
    // Another .txt file that contains the type name of a component to apply.
    [SerializeField] private TextAsset componentToApplyType;

    // -------------------------------------------------------------------
    // PROPERTIES that wrap the logic of reading from the .txt to a System.Type.
    // -------------------------------------------------------------------
    public virtual Type EffectControllerType => ResolveTypeFromText(effectControllerType);
    public virtual Type ComponentToApplyType => ResolveTypeFromText(componentToApplyType);

    [Header("Visual Effect Settings")]
    public Sprite icon;
    public List<VFX> vfxList = new List<VFX>();

    [Header("Level Settings")]
    // When needsLevel is false the effect is applied with its base values.
    // If needsLevel is true then currentLevel is maintained and upgraded.
    public bool needsLevel = false;
    // If currentLevel is 0 the effect has not been applied yet.
    public int currentLevel = 0;
    // List of upgradeable parameters. Each entry defines the field name (attributeName)
    // and the fixed amount (attributeValue) to add for every upgrade.
    public List<UpgradeableField> upgradableParameters;

    // A dictionary to cache the original (base) values of the upgradeable fields.
    private Dictionary<string, float> baseValues = new Dictionary<string, float>();

    // Cache base values when the asset is enabled.
    private void OnEnable()
    {
        CacheBaseValues();
    }

    // This method attempts to read and store the base values for each upgradeable parameter.
    private void CacheBaseValues()
    {
        baseValues.Clear();
        foreach (var upField in upgradableParameters)
        {
            FieldInfo fieldInfo = this.GetType().GetField(upField.attributeName, BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfo != null && fieldInfo.FieldType == typeof(float))
            {
                float value = (float)fieldInfo.GetValue(this);
                baseValues[upField.attributeName] = value;
            }
            else
            {
                Debug.LogWarning($"{name}: Field '{upField.attributeName}' not found or not a float.");
            }
        }
    }

    // Execute the effect by adding its controller to the effected game object.
    public virtual void ExecuteEffect(IEffectableController effectedGameController)
    {
        Type controllerType = EffectControllerType;
        if (controllerType == null)
        {
            Debug.LogError("Effect controller type not set or invalid!");
            return;
        }

        var monoObj = effectedGameController as MonoBehaviour;
        if (monoObj == null)
        {
            Debug.LogError("Effected object is not a MonoBehaviour. Can't add component.");
            return;
        }

        EffectController effectController = monoObj.gameObject.AddComponent(controllerType) as EffectController;
        if (effectController != null)
        {
            effectController.Initialize(this);
        }
        else
        {
            Debug.LogError($"Failed to add effect controller {controllerType.FullName}.");
        }
    }

    // Initialize the effect. If the effect has never been applied (currentLevel is 0) or does not require leveling,
    // it is applied at its base (level 1) values. Otherwise, the upgrade logic is invoked.
    public virtual void InitializeEffectObject()
    {
        if (currentLevel == 0 || !needsLevel)
        {
            currentLevel = 1;
            OnInitializeEffect();
        }
        else
        {
            UpgradeEffect();
        }
    }

    protected virtual void OnInitializeEffect()
    {
        Type comp = ComponentToApplyType;
        if (comp == null)
        {
            Debug.LogWarning("No componentToApply was chosen.");
            return;
        }

        EffectEvents.ApplyEffect(this, comp);
    }

    // Upgrade the effect by increasing its level and updating each upgradeable field.
    public virtual void UpgradeEffect()
    {
        // Increase the current level.
        currentLevel++;
        OnUpgradeEffect();
    }

    protected virtual void OnUpgradeEffect()
    {
        // For every field that can be upgraded, add the fixed increment.
        foreach (var upField in upgradableParameters)
        {
            FieldInfo fieldInfo = this.GetType().GetField(upField.attributeName, BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfo != null && fieldInfo.FieldType == typeof(float))
            {
                float currentValue = (float)fieldInfo.GetValue(this);
                float newValue = currentValue + upField.attributeValue;
                fieldInfo.SetValue(this, newValue);
                Debug.Log($"{name}: Upgraded '{upField.attributeName}' from {currentValue} to {newValue}");
            }
            else
            {
                Debug.LogWarning($"{name}: Field '{upField.attributeName}' not found or is not a float.");
            }
        }
    }

    // Reset the effect to level 1 by restoring the base values of all upgradeable fields.
    public virtual void ResetUpgrade()
    {
        currentLevel = 1;
        foreach (var upField in upgradableParameters)
        {
            if (baseValues.TryGetValue(upField.attributeName, out float baseValue))
            {
                FieldInfo fieldInfo = this.GetType().GetField(upField.attributeName, BindingFlags.Public | BindingFlags.Instance);
                if (fieldInfo != null && fieldInfo.FieldType == typeof(float))
                {
                    fieldInfo.SetValue(this, baseValue);
                    Debug.Log($"{name}: Reset '{upField.attributeName}' to its base value of {baseValue}");
                }
            }
        }
    }

    // PRIVATE method that converts a TextAsset into a System.Type.
    protected Type ResolveTypeFromText(TextAsset textAsset)
    {
        if (textAsset == null) return null;

        string rawTypeName = textAsset.name.Trim();
        if (string.IsNullOrEmpty(rawTypeName)) return null;

        // Attempt to get the type by the name stored in the .txt file.
        return Type.GetType(rawTypeName);
    }
}

[Serializable]
public class UpgradeableField
{
    // Name of the field on the effect object that will be upgraded (for example, "duration" or "tickDuration").
    public string attributeName;
    // The fixed amount to add to that field for each upgrade.
    public float attributeValue;
}