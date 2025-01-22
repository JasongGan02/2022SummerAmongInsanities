using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(CharacterSpawnManager))]
    public class CharacterSpawnManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // Draw the default inspector

            CharacterSpawnManager spawnManager = (CharacterSpawnManager)target;
            if (GUILayout.Button("Spawn Random Enemy"))
            {
                spawnManager.SpawnRandomEnemy();
            }
            if (GUILayout.Button("Spawn Many Enemies"))
            {
                spawnManager.SpawnManyEnemy();
            }
            if (GUILayout.Button("Spawn Bat"))
            {
                spawnManager.SpawnBat();
            }
            if (GUILayout.Button("Spawn Villager"))
            {
                spawnManager.SpawnVillager();
            }
            if (GUILayout.Button("Spawn Lady"))
            {
                spawnManager.SpawnLady();
            }
            if (GUILayout.Button("Spawn Dumb"))
            {
                spawnManager.SpawnDumb();
            }
            if (GUILayout.Button("Spawn VillagerWithWeapon"))
            {
                spawnManager.SpawnVillagerWithWeapon();
            }
            if (GUILayout.Button("Spawn Creeper"))
            {
                spawnManager.SpawnCreeper();
            }
            if (GUILayout.Button("Group Attack Player"))
            {
                spawnManager.GroupCommand("attack player");
            }
            if (GUILayout.Button("Group Stop Attack Player"))
            {
                spawnManager.GroupCommand("not attack player");
            }
            if (GUILayout.Button("Group Attack core"))
            {
                spawnManager.GroupCommand("attack core");
            }
            if (GUILayout.Button("Group Stop Attack core"))
            {
                spawnManager.GroupCommand("not attack core");
            }
        }
    }
}
