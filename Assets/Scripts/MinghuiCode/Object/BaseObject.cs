using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections.Generic;

public class BaseObject : ScriptableObject
{
    public string itemName;
    public float sizeRatio = 1;

    /*
     * It has the default sprite and collider
     * It does not have any script
     */
    [SerializeField]
    protected GameObject prefab;

    /**
     * set itemName to fileName
     */

    public Sprite getPrefabSprite()
    {
        return prefab.GetComponent<SpriteRenderer>().sprite;
    }
    private void OnValidate()
    {
        itemName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this));
    }
}

