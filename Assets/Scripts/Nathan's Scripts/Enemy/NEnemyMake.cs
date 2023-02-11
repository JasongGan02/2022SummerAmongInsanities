using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NEnemyMake : MonoBehaviour
{
    public CharacterAtlas characterAtlas;
    CoreArchitecture coreArchitecture;

    private float time;
    public int max;

    void Awake()
    {
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
        SpawnVillager();
        SpawnPlayer();
        SpawnBat();
        time = 0;
    }
    void Start()
    {

    }

    private void Update()
    {
        time += Time.deltaTime;
        if (time > 3 && !TooMuch(max)) 
        {
            SpawnBat();
            SpawnVillager();
            time = 0;
        }

    }

    public bool TooMuch(int max)
    {
        if (GameObject.FindGameObjectsWithTag("enemy").Length > max)
        {
            return true;
        }

        return false;
    }

    public void SpawnVillager()
    {
        GameObject villagerGameObject = characterAtlas.villager.GetSpawnedGameObject();
        villagerGameObject.transform.position = GetComponentInChildren<Transform>().position;
        villagerGameObject.transform.parent = GameObject.Find("EnemyContainer").transform;
    }

    public void SpawnPlayer()
    {
        GameObject playerGameObject = characterAtlas.player.GetSpawnedGameObject();
        playerGameObject.transform.position = coreArchitecture.GetComponent<Transform>().position;
    }

    public void SpawnBat()
    {
        GameObject BatGameObject = characterAtlas.bat.GetSpawnedGameObject();
        BatGameObject.transform.position = GetComponentInChildren<Transform>().position;
        BatGameObject.transform.parent = GameObject.Find("EnemyContainer").transform;
    }
}
