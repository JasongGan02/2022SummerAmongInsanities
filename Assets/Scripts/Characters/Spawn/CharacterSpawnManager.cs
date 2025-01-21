using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawnManager : MonoBehaviour
{
    public CharacterAtlas characterAtlas;
    public EnemyObject[] enemyTypes;
    private float timer;
    private int deathCount = 0;
    private CoreArchitectureController coreArchitecture;
    private GameObject player;
    private Vector3 coreSpawnPosition;
    public Transform coreTransform = null;

    private void Start()
    {
        StartCoroutine(WaitForCoreArchitectureAndInitialize());
        StartCoroutine(CollectEnemiesPeriodically());
    }

    protected virtual void FixedUpdate()
    {

        if (Input.GetKeyDown(KeyCode.H))
        {
            //SpawnRandomEnemy();
        }

        
    }

    public void SpawnCharacter(CharacterObject characterType)
    {
        
        GameObject character = PoolManager.Instance.Get(characterType);

        if (character != null)
        {
            if (characterType is EnemyObject)
            {
                character.transform.position = GetSpawnPosition();
                character.transform.parent = GameObject.Find("EnemyContainer").transform;
            }
            else if (characterType is PlayerObject) { }
            
        }
        else
        {
            Debug.LogError("Prefab not found for enemy type: " + characterType.ToString());
        }
    }

    private Vector3 GetSpawnPosition()
    {
        // this is for test
        Vector3 target = Vector3.zero;
        player = GameObject.Find("Player");
        target = player.transform.position + new Vector3(10, 2, 0);
        return target;
    }

    public void StartRespawnCoroutine()
    {
        StartCoroutine(RespawnPlayerCoroutine());
    }

    private IEnumerator RespawnPlayerCoroutine()
    {
        // Implement the respawn logic here
        // For example, waiting for a certain time before respawning
        yield return new WaitForSeconds(((PlayerStats)characterAtlas.player.maxStats).respawnTimeInterval);

        SpawnPlayer();
    }
    private void SpawnPlayer()
    {
        GameObject player = PoolManager.Instance.Get(characterAtlas.player);
        player.transform.position = coreSpawnPosition;
        this.player = player;
    }

    IEnumerator WaitForCoreArchitectureAndInitialize()
    {
        while (CoreArchitectureController.Instance == null)
        {
            yield return null;
        }

        coreArchitecture = CoreArchitectureController.Instance;
        coreSpawnPosition = coreArchitecture.transform.Find("SpawnPoint").position;
        transform.position = coreSpawnPosition;
        coreTransform = transform;
        SpawnPlayer();
    }

    public void SpawnRandomEnemy()
    {
        // Assuming you have a list or array of enemy types
        int randomIndex = UnityEngine.Random.Range(0, enemyTypes.Length);
        CharacterObject randomEnemyType = enemyTypes[randomIndex];
        SpawnCharacter(randomEnemyType);
    }
    public IEnumerator CollectEnemiesPeriodically()
    {
        while (true) // This loop will run indefinitely
        {
            CollectEnemies();
            yield return new WaitForSeconds(3); // Wait for 20 seconds before the next check
        }
    }
    private List<GameObject> enemies = new List<GameObject>();
    private void CollectEnemies()
    {
        // Find all game objects tagged as "enemy"
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("enemy");
        enemies.Clear(); // Clear the list to avoid duplicates

        // Check each enemy if it's under any "EnemyContainer"
        foreach (GameObject enemy in allEnemies)
        {
            Transform parent = enemy.transform.parent;
            while (parent != null) // Check up the hierarchy
            {
                if (parent.name == "EnemyContainer")
                {
                    enemies.Add(enemy);
                    break; // Stop the search as we have found the enemy under the required parent
                }
                parent = parent.parent; // Move up in the hierarchy
            }
        }

        // Debug to see how many enemies were collected
        //Debug.Log("Collected " + enemies.Count + " enemies under EnemyContainer(s).");
    }

    public void GroupCommand(string command)
    {
        if (enemies.Count <= 0) { return; }
        if (command == "attack player")
        {
            CommandMove(player.transform);
        }
        else if (command == "attack core")
        {
            CommandMove(coreArchitecture.transform);
        }
        else
        {
            StopGroupCommand();
        }
    }

    public void CommandMove(Transform targetTransform)
    {
        foreach (GameObject enemy in enemies)
        {
            var enemyController = enemy.GetComponent<BatController>();
            if (enemyController != null)
            {
                enemyController.IsGroupAttacking = true;  // Set the flag when commanding to move
                //enemyController.MoveTowards(targetTransform);  // This function needs to be defined or adjusted accordingly
            }
        }
    }
    public void StopGroupCommand()
    {
        foreach (GameObject enemy in enemies)
        {
            var enemyController = enemy.GetComponent<BatController>();
            if (enemyController != null)
            {
                enemyController.IsGroupAttacking = false;  // Clear the flag to resume normal behavior
            }
        }
    }

    public void SpawnManyEnemy()
    {
        for (int i = 0; i < 30; i++)
        {
            SpawnRandomEnemy();
        }
    }
    public void SpawnBat()
    {
        CharacterObject enemy = enemyTypes[0];
        SpawnCharacter(enemy);
    }
    public void SpawnVillager()
    {
        CharacterObject enemy = enemyTypes[2];
        SpawnCharacter(enemy);
    }
    public void SpawnLady()
    {
        CharacterObject enemy = enemyTypes[1];
        SpawnCharacter(enemy);
    }
    public void SpawnDumb()
    {
        CharacterObject enemy = enemyTypes[3];
        SpawnCharacter(enemy);
    }
    public void SpawnVillagerWithWeapon()
    {
        CharacterObject enemy = enemyTypes[4];
        SpawnCharacter(enemy);
    }
    public void SpawnCreeper()
    {
        CharacterObject enemy = enemyTypes[5];
        SpawnCharacter(enemy);
    }
}
