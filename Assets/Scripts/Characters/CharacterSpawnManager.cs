using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawnManager : MonoBehaviour
{
    public CharacterAtlas characterAtlas;
    CoreArchitecture coreArchitecture;

    void Start()
    {
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
        spawnPlayer();
    }

    public void spawnPlayer()
    {
        GameObject playerGameObject = characterAtlas.player.GetSpawnedCharacter();
        playerGameObject.transform.position = coreArchitecture.transform.position;
        Debug.Log(playerGameObject.transform.position);
    }
}
