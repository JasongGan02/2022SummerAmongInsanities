using System.Collections.Generic;
using UnityEngine;

public class TerrainRenderer : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private int RenderDistance;
    private WorldGenerator GeneratorInstance;

    void Start()
    {
        GeneratorInstance = GetComponent<WorldGenerator>();
        //InitializeFiniteWorld();
    }

    /*void InitializeFiniteWorld()
    {
        // Generate data for all chunks within the finite world size
        GeneratorInstance.dataCreator.GenerateAllWorldData(worldSizeInChunks);

        // Optionally, generate light data for all chunks
        LightGenerator.Instance.GenerateAllLightData(worldSizeInChunks);
    }*/

    void Update()
    {
        // Only handle rendering of chunks when the player is near them
        if (Player == null)
        {
            Player = FindObjectOfType<PlayerController>()?.transform;
            return;
        }

        int plrChunkX = (int)(Player.position.x / WorldGenerator.ChunkSize.x);
        for (int x = plrChunkX - RenderDistance; x <= plrChunkX + RenderDistance; x++)
        {
            if (!WorldGenerator.ActiveChunks.ContainsKey(x))
            {
                StartCoroutine(GeneratorInstance.CreateChunk(x));
            }
        }

        // Disable chunks outside render distance
        List<int> chunksToDisable = new List<int>();
        foreach (var activeChunk in WorldGenerator.ActiveChunks)
        {
            int chunkX = activeChunk.Key;
            if (chunkX < plrChunkX - RenderDistance || chunkX > plrChunkX + RenderDistance)
            {
                chunksToDisable.Add(chunkX);
            }
        }

        foreach (int chunkX in chunksToDisable)
        {
            WorldGenerator.ActiveChunks[chunkX].SetActive(false);
            WorldGenerator.ActiveChunks.Remove(chunkX);
        }
    }
}
