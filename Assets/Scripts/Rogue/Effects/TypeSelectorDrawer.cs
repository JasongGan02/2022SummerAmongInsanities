using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MonoScript))]
public class TypeSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Type baseType = null;

        // Determine which field is being drawn and set the appropriate base type for filtering
        if (property.name == "effectControllerType")
        {
            baseType = typeof(EffectController);
        }
        else if (property.name == "componentToApply")
        {
            baseType = typeof(MonoBehaviour); // Replace AnotherBaseType with your specific type
        }

        if (baseType == null)
        {
            EditorGUI.LabelField(position, label.text, "Invalid type filter");
            return;
        }

        // Get all types that are subclasses of the determined base type
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
            .ToList();

        // Create a list of type names for the dropdown
        var typeNames = types.Select(t => t.FullName).ToArray();

        // Get the currently selected type from the MonoScript
        var currentType = property.objectReferenceValue as MonoScript;
        int selectedIndex = Array.IndexOf(typeNames, currentType != null ? currentType.GetClass().FullName : null);

        // Draw the dropdown
        selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, typeNames);

        // Assign the selected type back to the MonoScript field
        if (selectedIndex >= 0 && selectedIndex < types.Count)
        {
            var selectedType = types[selectedIndex];

            if (typeof(MonoBehaviour).IsAssignableFrom(selectedType))
            {
                // Handle MonoBehaviour type
                var tempGameObject = new GameObject("Temp");
                var monoBehaviour = tempGameObject.AddComponent(selectedType) as MonoBehaviour;
                property.objectReferenceValue = MonoScript.FromMonoBehaviour(monoBehaviour);
                GameObject.DestroyImmediate(tempGameObject);
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(selectedType))
            {
                // Handle ScriptableObject type
                var scriptableObject = ScriptableObject.CreateInstance(selectedType);
                property.objectReferenceValue = MonoScript.FromScriptableObject(scriptableObject);
                ScriptableObject.DestroyImmediate(scriptableObject);
            }
        }
        else
        {
            property.objectReferenceValue = null;
        }
    }
}
