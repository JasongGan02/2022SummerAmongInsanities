using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(SpawnManager))]
    public class SpawnManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SpawnManager spawnManager = (SpawnManager)target;
            if (GUILayout.Button("Spawn random weapon"))
            {
                spawnManager.SpawnRamdonWeapon();
            }
            if (GUILayout.Button("Spawn debug initial ash"))
            {
                spawnManager.SpawnDebugInitialAsh();
            }
        }
    }
}