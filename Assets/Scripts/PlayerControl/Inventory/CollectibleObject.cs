using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "newCollectibleObject", menuName = "Collectible Object")]
public class CollectibleObject : ScriptableObject
{
    public string itemName;
    public int maxStack;
    public ItemType itemType;
    public float sizeRatio;
    public GameObject droppedItem;

    [HideInInspector]
    public int healthPoint;

    [HideInInspector]
    public int damage;
}

public enum ItemType
{
    Comsumable,
    Equipment,
    Material,
    Misc
}

[CustomEditor(typeof(CollectibleObject))]
public class CollectibleObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CollectibleObject collectibleObject = (CollectibleObject)target;

        switch (collectibleObject.itemType) // if bool is true, show other fields
        {
            case ItemType.Comsumable:
                collectibleObject.healthPoint = EditorGUILayout.IntField("Health Point", collectibleObject.healthPoint);
                break;
            case ItemType.Equipment:
                collectibleObject.damage = EditorGUILayout.IntField("Damage", collectibleObject.damage);
                break;
            default:
                break;
        }


    }
}
