using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    public CharacterAtlas characterAtlas;
    CoreArchitecture coreArchitecture;
    float RespwanTimeInterval;
    float timer;

    void Awake()
    {
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
        RespwanTimeInterval = (characterAtlas.player as PlayerObject).RespwanTimeInterval;
        SpawnVillager();
        SpawnPlayer();
    }
    void Start()
    {
        
    }

    protected void Update()
    {
        if(GameObject.Find("Player")==null)
        {
            timer += Time.deltaTime;
            if(timer >= RespwanTimeInterval)
            {
                SpawnPlayer();
                timer = 0f;
            }
        }
    }


    public void SpawnVillager()
    {
        GameObject villagerGameObject = characterAtlas.villager.GetSpawnedGameObject();
        villagerGameObject.transform.position  = GetComponentInChildren<Transform>().position;
        villagerGameObject.transform.parent = GameObject.Find("EnemyContainer").transform;
    }

    public void SpawnPlayer()
    {
        GameObject playerGameObject = characterAtlas.player.GetSpawnedGameObject();
        playerGameObject.transform.position = coreArchitecture.GetComponent<Transform>().position;
        GameObject.FindObjectOfType<UIViewStateManager>().enabled = true;
    }

    public void SpawnBat()
    {
        GameObject BatGameObject = characterAtlas.bat.GetSpawnedGameObject();
        BatGameObject.transform.position = GetComponentInChildren<Transform>().position;
        BatGameObject.transform.parent = GameObject.Find("EnemyContainer").transform;
    }

    public bool TooMuch(int max)
    {
        if (GameObject.FindGameObjectsWithTag("enemy").Length > max)
        {
            return true;
        }

        return false;
    }
}