#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Property drawer for the HideInDerivedInspectorAttribute.
/// Ensures the field is only shown in the base class Inspector.
/// </summary>
[CustomPropertyDrawer(typeof(HideInDerivedInspectorAttribute))]
public class HideInDerivedInspectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Check if the current object is a derived type
        if (property.serializedObject.targetObject.GetType() == fieldInfo.DeclaringType)
        {
            // Only draw if the current object is not derived
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Check if the current object is a derived type
        if (property.serializedObject.targetObject.GetType() == fieldInfo.DeclaringType)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        // Hide the field by setting its height to 0
        return 0f;
    }
}
#endif