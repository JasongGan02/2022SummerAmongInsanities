using System;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Editor
{
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

                    MonoScript monoScript = GetMonoScriptFromType(selectedType);
                    property.objectReferenceValue = monoScript;
                }
                else
                {
                    property.objectReferenceValue = null;
                }
            }
            else if (property.name == "componentToApply")
            {
                baseType = typeof(MonoBehaviour); // Replace with your specific type

                // Get the currently selected type from the MonoScript
                var currentType = property.objectReferenceValue as MonoScript;
                string typeName = currentType != null ? currentType.GetClass().FullName : "None";

                if (GUI.Button(position, new GUIContent(label.text + ": " + typeName, label.tooltip)))
                {
                    var typeDropdown = new TypeDropdown(new AdvancedDropdownState(), baseType, property);
                    typeDropdown.Show(position);
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Invalid type filter");
                return;
            }
        }

        private MonoScript GetMonoScriptFromType(Type type)
        {
            // Load all MonoScripts
            var monoScripts = Resources.FindObjectsOfTypeAll<MonoScript>();
            foreach (var monoScript in monoScripts)
            {
                if (monoScript.GetClass() == type)
                    return monoScript;
            }

            return null;
        }
    }

    public class TypeDropdownItem : AdvancedDropdownItem
    {
        public Type Type { get; private set; }

        public TypeDropdownItem(string name, Type type) : base(name)
        {
            this.Type = type;
        }
    }

    public class TypeDropdown : AdvancedDropdown
    {
        private Type baseType;
        private SerializedProperty property;

        public TypeDropdown(AdvancedDropdownState state, Type baseType, SerializedProperty property) : base(state)
        {
            this.baseType = baseType;
            this.property = property;
            minimumSize = new Vector2(200, 300);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Select Type");

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => baseType.IsAssignableFrom(t) && t.IsClass)
                .OrderBy(t => t.FullName)
                .ToList();

            foreach (var type in types)
            {
                var path = type.FullName.Replace('.', '/');
                AddTypeToTree(root, path, type);
            }

            return root;
        }

        private void AddTypeToTree(AdvancedDropdownItem root, string path, Type type)
        {
            var splitPath = path.Split('/');
            var currentRoot = root;

            for (int i = 0; i < splitPath.Length; i++)
            {
                var found = currentRoot.children.FirstOrDefault(c => c.name == splitPath[i]);
                if (found == null)
                {
                    AdvancedDropdownItem newChild;

                    if (i == splitPath.Length - 1)
                    {
                        // This is the leaf node, use TypeDropdownItem
                        newChild = new TypeDropdownItem(splitPath[i], type);
                    }
                    else
                    {
                        newChild = new AdvancedDropdownItem(splitPath[i]);
                    }

                    currentRoot.AddChild(newChild);
                    currentRoot = newChild;
                }
                else
                {
                    currentRoot = found;
                }
            }
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item == null)
                return;

            Type selectedType = null;

            if (item is TypeDropdownItem typeItem)
            {
                selectedType = typeItem.Type;
            }

            if (selectedType != null)
            {
                MonoScript monoScript = GetMonoScriptFromType(selectedType);
                property.objectReferenceValue = monoScript;
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private MonoScript GetMonoScriptFromType(Type type)
        {
            // Load all MonoScripts
            var monoScripts = Resources.FindObjectsOfTypeAll<MonoScript>();
            foreach (var monoScript in monoScripts)
            {
                if (monoScript.GetClass() == type)
                    return monoScript;
            }

            return null;
        }
    }
}