using System.Collections;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CharacterSpawnManager : MonoBehaviour
{
    public CharacterAtlas characterAtlas;
    public EnemyObject[] enemyTypes;
    private float timer;
    private int deathCount = 0;
    private CoreArchitectureController coreArchitecture;
    private GameObject player;

    private void Start()
    {
        StartCoroutine(WaitForCoreArchitectureAndInitialize());
    }

    protected virtual void FixedUpdate()
    {

        if (Input.GetKeyDown(KeyCode.H))
        {
            SpawnRandomEnemy();
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
        yield return new WaitForSeconds(((PlayerObject)characterAtlas.player).RespwanTimeInterval);

        SpawnPlayer();
    }
    private void SpawnPlayer()
    {
        GameObject player = PoolManager.Instance.Get(characterAtlas.player);
        player.transform.position = coreArchitecture.transform.position;
        this.player = player;
    }

    IEnumerator WaitForCoreArchitectureAndInitialize()
    {
        while (CoreArchitectureController.Instance == null)
        {
            yield return null;
        }

        coreArchitecture = CoreArchitectureController.Instance;
        transform.position = coreArchitecture.transform.position;
        SpawnPlayer();
    }

    public void SpawnRandomEnemy()
    {
        // Assuming you have a list or array of enemy types
        int randomIndex = UnityEngine.Random.Range(0, enemyTypes.Length);
        CharacterObject randomEnemyType = enemyTypes[randomIndex];
        SpawnCharacter(randomEnemyType);
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

[CustomEditor(typeof(CharacterSpawnManager))]
public class CharacterSpawnManagerEditor : Editor
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
    }
}
