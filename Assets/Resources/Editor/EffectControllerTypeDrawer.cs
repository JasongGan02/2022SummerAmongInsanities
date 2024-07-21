using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MonoScript), true)]
public class EffectControllerTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        MonoScript monoScript = (MonoScript)property.objectReferenceValue;
        Type type = monoScript?.GetClass();

        if (type == null || !typeof(EffectController).IsAssignableFrom(type))
        {
            property.objectReferenceValue = null;
        }

        // ObjectField that only shows MonoScript objects
        MonoScript selectedScript = EditorGUI.ObjectField(position, label, monoScript, typeof(MonoScript), false) as MonoScript;
        if (selectedScript != null && typeof(EffectController).IsAssignableFrom(selectedScript.GetClass()))
        {
            property.objectReferenceValue = selectedScript;
        }

        EditorGUI.EndProperty();
    }
}