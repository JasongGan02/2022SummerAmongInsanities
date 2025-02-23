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

            GUILayout.Space(10);
            GUILayout.Label("Debug Spawning Functions", EditorStyles.boldLabel);

            if (GUILayout.Button("Spawn Equipment"))
            {
                spawnManager.SpawnEquipment();
            }

            if (GUILayout.Button("Spawn Towers"))
            {
                spawnManager.SpawnTowers();
            }

            if (GUILayout.Button("Spawn Ash"))
            {
                spawnManager.SpawnAsh();
            }

            if (spawnManager.isDebug)
            {
                if (GUILayout.Button("Spawn Random Weapon (Equipment + Towers)"))
                {
                    spawnManager.SpawnRandomWeapon();
                }
            }
            else
            {
                if (GUILayout.Button("Spawn Initial Items"))
                {
                    spawnManager.SpawnInitialItems();
                }
            }
        }
    }
}