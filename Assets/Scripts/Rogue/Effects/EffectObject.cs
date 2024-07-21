using System;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Effects")]
public abstract class EffectObject : ScriptableObject
{
    [Header("General Effect Settings")]
    public int cost;
    public int level;
    public float duration;
    public bool requiresReset = false;
    public bool isStackable = false;
    public bool isReselectable = true;
    public bool isPermanent = false;
    public string description; 
    public MonoScript effectControllerType;
    
    public virtual void ExecuteEffect(IEffectableController effectedGameController)
    {
        Type controllerType = GetEffectControllerType();
        if (controllerType == null)
        {
            Debug.LogError("Effect controller type not set or invalid.");
            return;
        }

        MonoBehaviour monoBehaviour = effectedGameController as MonoBehaviour;
        if (monoBehaviour == null)
        {
            Debug.LogError("Effected game object is not set or invalid.");
            return;
        }

        EffectController controller = monoBehaviour.gameObject.AddComponent(controllerType) as EffectController;
        if (controller == null)
        {
            Debug.LogError("Failed to add controller component.");
            return;
        }

        controller.Initialize(this);
    }

    public Type GetEffectControllerType()
    {
        return effectControllerType?.GetClass();
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(EffectObject), true)]
    public class EffectObjectEditor : Editor
    {
        private SerializedProperty effectControllerTypeProperty;

        private void OnEnable()
        {
            effectControllerTypeProperty = serializedObject.FindProperty("effectControllerType");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspectorExcept("effectControllerType");
        
            EditorGUILayout.PropertyField(effectControllerTypeProperty, new GUIContent("Effect Controller Type"), true);
        
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDefaultInspectorExcept(string propertyToExclude)
        {
            SerializedProperty prop = serializedObject.GetIterator();
            bool enterChildren = true;
            while (prop.NextVisible(enterChildren))
            {
                if (prop.name != propertyToExclude)
                {
                    EditorGUILayout.PropertyField(prop, true);
                }
                enterChildren = false;
            }
        }
    }
    #endif
}
