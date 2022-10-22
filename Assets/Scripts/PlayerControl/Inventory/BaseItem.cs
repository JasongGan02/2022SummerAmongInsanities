using UnityEngine;
using System.IO;
using UnityEditor;

public class BaseItem : ScriptableObject
{
    public string itemName;
    public int maxStack;
    public float sizeRatio;

    private void OnValidate()
    {
        itemName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this));
    }
}

