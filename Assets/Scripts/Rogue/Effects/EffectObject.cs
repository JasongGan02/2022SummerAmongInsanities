using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Effects")]
public abstract class EffectObject : BaseObject
{
    [Header("General Effect Settings")]

    public int cost;
    public int level;
    public float duration;
    public bool stackable;
    public bool repeatable; //If the effect can be selected multiple times
    public bool isPermanent;
    public string description;

    public virtual void ExecuteEffect(IEffectableObject effectedGameObject) //Use this when you are unsure about what type of controller will be using.
    {
        string controllerName = itemName+"Controller";
        Type type = Type.GetType(controllerName);
        var controller = (effectedGameObject as MonoBehaviour).gameObject.AddComponent(type);
        Debug.Log(controller);
        Debug.Log((effectedGameObject as MonoBehaviour).gameObject);
        (controller as EffectController).Initialize(this);
    }


    //Types of Effects: Spawn items, [permanent: Change of Character Object Stats, duration/permanent Change of Game Stat or special effects/new game mechanics], 
    //public abstract void ExecuteEffect(IEffectable character);

    /***
        IEffectableObject:
            - Effect List

        Effect
            
    ***/


    // Reference to the script component type
    public MonoScript applyingControllerType;

    // Get the script component type from the MonoScript reference
    public Type GetApplyingControllerType()
    {
        if (applyingControllerType != null)
        {
            return applyingControllerType.GetClass();
        }
        return null;
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

                // Draw the default fields of the scriptable object
                DrawDefaultInspector();

                // Draw a custom object field for selecting the script component type
                EditorGUILayout.PropertyField(applyingControllerTypeProperty, new GUIContent("Applying Controller Type"), true);

                serializedObject.ApplyModifiedProperties();
            }
        }
    #endif
}
