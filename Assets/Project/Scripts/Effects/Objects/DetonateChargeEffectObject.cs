using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Weapon/DetonateChargeEffect")]
public class DetonateChargeEffectObject : EffectObject
{
    [Header("Detonate Charge Properties")]
    public float chargeDuration; // Time required to fully charge
    public OnDetonateEffectObject detonateEffect; // Reference to the detonate effect

    public override void ExecuteEffect(IEffectableController effectedGameController)
    {
        var weapon = effectedGameController as MonoBehaviour;
        var effectController = weapon.GetComponent<DetonateChargeEffectController>();
        if (effectController == null)
        {
            // Add the DetonateChargeEffectController to the weapon instance
            var controller = weapon.gameObject.AddComponent<DetonateChargeEffectController>();
            controller.Initialize(this);
        }
    }
    
    
    public override void InitializeEffectObject()
    {
        // Apply the effect to all weapon objects
        WeaponObject[] allWeaponObjects = LoadAllAssets<WeaponObject>();
        foreach (var weaponObject in allWeaponObjects)
        {
            if (!weaponObject.onInitializeEffects.Contains(this))
            {
                weaponObject.onInitializeEffects.Add(this);
                //Debug.Log($"Added {name} to weapon object: {weaponObject.name}");
            }
            //TODO: if weapon objcet is ranged, then the projectile will carry the effect.
        }
        // Apply the effect to all existing weapon instances in the scene
        Weapon[] activeWeapons = Object.FindObjectsOfType<Weapon>();
        foreach (var weapon in activeWeapons)
        {
            AddEffectToWeaponInstance(weapon);
        }
    }

    private void AddEffectToWeaponInstance(Weapon weapon)
    {
        // Check if the weapon already has the effect
        var effectController = weapon.GetComponent<DetonateChargeEffectController>();
        if (effectController == null)
        {
            // Add the DetonateChargeEffectController to the weapon instance
            var controller = weapon.gameObject.AddComponent<DetonateChargeEffectController>();
            controller.Initialize(this);
            //Debug.Log($"Added {name} effect to weapon instance: {weapon.name}");
        }
    }

    private T[] LoadAllAssets<T>() where T : ScriptableObject
    {
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        List<T> assets = new List<T>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets.ToArray();
#else
        Debug.LogError("LoadAllAssets is only supported in the Unity Editor.");
        return null;
#endif
    }
}
