using System;
using UnityEngine;

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
    public bool stackable;
    public bool repeatable;
    public bool isPermanent;
    public string description;
    public MonoScript applyingControllerType;

    public virtual void ExecuteEffect(IEffectableObject effectedGameObject)
    {
        Type controllerType = GetApplyingControllerType();
        if (controllerType == null)
        {
            Debug.LogError("Controller type not set or invalid.");
            return;
        }

        MonoBehaviour monoBehaviour = effectedGameObject as MonoBehaviour;
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

    public Type GetApplyingControllerType()
    {
        return applyingControllerType?.GetClass();
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(EffectObject))]
    public class EffectObjectEditor : Editor
    {
        private SerializedProperty applyingControllerTypeProperty;

        private void OnEnable()
        {
            applyingControllerTypeProperty = serializedObject.FindProperty("applyingControllerType");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            EditorGUILayout.PropertyField(applyingControllerTypeProperty, new GUIContent("Applying Controller Type"));

            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}
