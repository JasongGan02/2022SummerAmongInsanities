
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterSpawnerManager))]
public class CharacterSpawnerManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CharacterSpawnerManager generator = (CharacterSpawnerManager)target;

        GUILayout.Space(10);


        if (GUILayout.Button("Spawn Enemies"))
        {
            generator.SpawnEnemies();
        }
    }
}
