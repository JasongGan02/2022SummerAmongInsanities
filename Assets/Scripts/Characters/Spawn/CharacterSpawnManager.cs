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
        target = player.transform.position + new Vector3(5, 3, 0);
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
    }
}
