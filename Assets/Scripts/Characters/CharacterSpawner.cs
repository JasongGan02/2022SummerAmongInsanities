using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    public CharacterAtlas characterAtlas;
    CoreArchitecture coreArchitecture;

    void Awake()
    {
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
        SpawnVillager();
        SpawnPlayer();
    }
    void Start()
    {
        
    }

    public void SpawnVillager()
    {
        GameObject villagerGameObject = characterAtlas.villager.GetSpawnedGameObject();
        villagerGameObject.transform.position  = GetComponentInChildren<Transform>().position;
    }

    public void SpawnPlayer()
    {
        GameObject playerGameObject = characterAtlas.player.GetSpawnedGameObject();
        playerGameObject.transform.position = coreArchitecture.GetComponent<Transform>().position;
    }
}
