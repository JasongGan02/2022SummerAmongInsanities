using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour, IDataPersistence
{
    public CharacterAtlas characterAtlas;
    CoreArchitecture coreArchitecture;
    public enum EnemyType { Villager, Bat, Lady, Dumb }
    float RespwanTimeInterval;
    float timer;
    private int deathCount = 0;

    private void Start()
    {
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
        RespwanTimeInterval = (characterAtlas.player as PlayerObject).RespwanTimeInterval;
        SpawnPlayer();
        //SpawnBat();
        //SpawnVillager();
        //SpawnLady();

    }

    public void LoadData(GameData data)
    {
        this.deathCount = data.deathCount;
    }

    public void SaveData(GameData data)
    {
        data.deathCount = this.deathCount;
    }

    protected void Update()
    {
        if (GameObject.Find("Player") == null)
        {
            timer += Time.deltaTime;
            if (timer >= RespwanTimeInterval)
            {
                SpawnPlayer();
                timer = 0f;
                deathCount++;
            }
        }
    }

   
     public void SpawnRandomEnemy()
    {
        EnemyType randomEnemyType = (EnemyType)Random.Range(0, 4); // Change '5' to the number of enemy types

        switch (randomEnemyType)
        {
            case EnemyType.Villager:
                SpawnVillager();
                break;
            case EnemyType.Bat:
                SpawnBat();
                break;
            case EnemyType.Lady:
                SpawnLady();
                break;
            case EnemyType.Dumb:
                SpawnDumb();
                break;
            default:
                Debug.LogError("Unknown enemy type: " + randomEnemyType);
                break;
        }
    }

    public void SpawnVillager()
        {
            GameObject villagerGameObject = characterAtlas.villager.GetSpawnedGameObject<VillagerController>();
            villagerGameObject.transform.position  = GetComponentInChildren<Transform>().position;
            villagerGameObject.transform.parent = GameObject.Find("EnemyContainer").transform;
        }

    public void SpawnPlayer()
    {
        GameObject playerGameObject = characterAtlas.player.GetSpawnedGameObject<PlayerController>();
        playerGameObject.transform.position = coreArchitecture.GetComponent<Transform>().position;
        GameObject.FindObjectOfType<UIViewStateManager>().enabled = true;
    }

    public void SpawnVillagerWithWeapon()
    {
        GameObject VillagerWithWeaponGameObject = characterAtlas.villagerWithWeapon.GetSpawnedGameObject<VillagerWithWeaponController>();
        VillagerWithWeaponGameObject.transform.position = GetComponentInChildren<Transform>().position;
        VillagerWithWeaponGameObject.transform.parent = GameObject.Find("EnemyContainer").transform;
    }

    public void SpawnBat()
    {
        GameObject BatGameObject = characterAtlas.bat.GetSpawnedGameObject<BatController>();
        BatGameObject.transform.position = GetComponentInChildren<Transform>().position;
        BatGameObject.transform.parent = GameObject.Find("EnemyContainer").transform;
    }
    public void SpawnLady()
    {
        GameObject LadyGameObject = characterAtlas.lady.GetSpawnedGameObject<LadyController>();
        LadyGameObject.transform.position = GetComponentInChildren<Transform>().position;
        LadyGameObject.transform.parent = GameObject.Find("EnemyContainer").transform;
    }
    public void SpawnDumb()
    {
        GameObject DumbGameObject = characterAtlas.dumb.GetSpawnedGameObject<DumbController>();
        DumbGameObject.transform.position = GetComponentInChildren<Transform>().position;
        DumbGameObject.transform.parent = GameObject.Find("EnemyContainer").transform;
    }

    public bool TooMuch(float max)
    {
        if (GameObject.FindGameObjectsWithTag("enemy").Length > max)
        {
            return true;
        }

        return false;
    }
}