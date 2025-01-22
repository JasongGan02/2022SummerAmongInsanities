using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
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

    public GameObject getPrefab()
    { 
        return prefab; 
    }


    public Sprite getPrefabSprite()
    {
        return prefab.GetComponent<SpriteRenderer>().sprite;
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Only run this code in the Editor
        itemName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this));
    }
#endif
}

