using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    CharacterAtlas characterAtlas;

    void Start()
    {
        characterAtlas.villager.GetSpawnedGameObject();
    }
}
