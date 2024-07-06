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
    public bool isRepeatable = true;
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
            Debug.LogError("Effected game object is not a MonoBehaviour.");
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
    [CustomEditor(typeof(EffectObject))]
    public class EffectObjectEditor : Editor
    {
        private SerializedProperty effectControllerTypeProperty;
        
        private void OnEnable()
        {
            effectControllerTypeProperty = serializedObject.FindProperty("effectControllerTypeProperty");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            
            EditorGUILayout.PropertyField(effectControllerTypeProperty, new GUIContent("Effect Controller Type"));
            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}
