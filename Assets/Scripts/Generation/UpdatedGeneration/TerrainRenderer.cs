using System.Collections.Generic;
using UnityEngine;

public class TerrainRenderer : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private int RenderDistance;
    private WorldGenerator GeneratorInstance;
    private HashSet<int> chunksBeingGenerated = new HashSet<int>();

    void Start()
    {
        GeneratorInstance = GetComponent<WorldGenerator>();
    }
    
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
            int xIndex = x;
            if (!WorldGenerator.ActiveChunks.ContainsKey(xIndex) && !chunksBeingGenerated.Contains(xIndex))
            {
                chunksBeingGenerated.Add(xIndex);
                StartCoroutine(GeneratorInstance.CreateChunk(xIndex, () => { chunksBeingGenerated.Remove(xIndex); }));
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
