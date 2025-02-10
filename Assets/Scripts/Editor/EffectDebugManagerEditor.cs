#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EffectDebugManager))]
public class EffectDebugManagerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector interface.
        DrawDefaultInspector();

        // Get a reference to the target script.
        EffectDebugManager debugManager = (EffectDebugManager)target;

        // Add a button to the inspector.
        if (GUILayout.Button("Reset All Effects"))
        {
            // Call the ResetAllEffects() method when the button is clicked.
            debugManager.ResetAllEffects();
        }
    }
}
#endif