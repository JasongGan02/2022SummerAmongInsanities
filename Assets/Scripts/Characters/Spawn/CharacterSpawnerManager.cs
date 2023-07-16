using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawnerManager : MonoBehaviour
{
    public CharacterSpawner characterSpawner; // Reference to the CharacterSpawner script
    public int numberOfEnemiesToSpawn = 5;    // Number of enemies to spawn

    private void Update()
    {
        // Check if the user pressed the spawn button (for testing purposes, you can change this trigger condition)
        if (Input.GetKeyDown(KeyCode.H))
        {
            SpawnEnemies();
        }
    }

    public void SpawnEnemies()
    {
        // Spawn the specified number of enemies
        for (int i = 0; i < numberOfEnemiesToSpawn; i++)
        {
            characterSpawner.SpawnRandomEnemy();
        }
    }
}
