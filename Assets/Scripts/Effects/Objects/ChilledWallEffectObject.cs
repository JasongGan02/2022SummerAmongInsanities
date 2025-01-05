using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Tower Upgrades/ChilledWallEffectObject")]
public class ChilledWallEffectObject : EffectObject
{
    [Header("ChilledWallEffectObject Fields")] 
    public int maxStacks;
    public float armorMultiplier;
    
    
    public override void ExecuteEffectOnAType()
    {
        // Load all wall objects
        WallObject[] walls = LoadAllAssets<WallObject>();
        if (walls == null || walls.Length == 0)
        {
            Debug.LogWarning("No wall objects found.");
            return;
        }

        foreach (var wall in walls)
        {
            var existingEffect = wall.onHitEffects.Find(effect => effect.GetType() == GetType());
            if (existingEffect == null)
            {
                wall.onHitEffects.Add(this);
            }
            else
            {
                //((IUpgradeableEffectObject)existingEffect).UpgradeLevel();
            }
        }
    }

    private T[] LoadAllAssets<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        List<T> assets = new List<T>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) assets.Add(asset);
        }

        return assets.ToArray();
    }
}
