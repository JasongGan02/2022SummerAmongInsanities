using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrainGenerator : MonoBehaviour
{

    [SerializeField] private Transform Player;
    [SerializeField] private int RenderDistance;
    [SerializeField] private int smoothIteration = 2;
    private WorldGenerator GeneratorInstance;
    private List<int> CoordsToRemove;
    private int previousPlrChunkX = 0;
    int plrChunkX;
    private int createChunkCounter = 0;
    // Start is called before the first frame update
    void Start()
    {
        GeneratorInstance = GetComponent<WorldGenerator>();
        CoordsToRemove = new List<int>();
        if (Player == null)
        {
            InitializeTerrainAtDefaultPosition();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Player == null)
        {
            Player = FindObjectOfType<PlayerController>()?.gameObject.transform ?? null;
            return;
        }

        plrChunkX = (int)Player.position.x / WorldGenerator.ChunkSize.x;
        CoordsToRemove.Clear();

        foreach (KeyValuePair<int, GameObject> activeChunk in WorldGenerator.ActiveChunks)
        {
            CoordsToRemove.Add(activeChunk.Key);
        }

        for (int x = plrChunkX - RenderDistance; x <= plrChunkX + RenderDistance; x++)
        {
            int chunkCoord = x;
            if (!WorldGenerator.ActiveChunks.ContainsKey(chunkCoord))
            {
                createChunkCounter++; // Increment the counter
                StartCoroutine(CreateChunkWithCallback(chunkCoord, OnChunkCreated));
            }

            CoordsToRemove.Remove(chunkCoord);
        }

        foreach (int coord in CoordsToRemove)
        {
            GameObject chunkToDisable = WorldGenerator.ActiveChunks[coord];
            WorldGenerator.ActiveChunks.Remove(coord);
            chunkToDisable.SetActive(false);
        }
        UpdateLightingAfterChunksCreated();
        previousPlrChunkX = plrChunkX;
    }

    private void OnChunkCreated()
    {
        createChunkCounter--; // Decrement the counter
        Debug.Log(createChunkCounter);
        if (createChunkCounter == 0) // All CreateChunk coroutines have finished
        {
            UpdateLightingAfterChunksCreated();
        }
    }

    private void UpdateLightingAfterChunksCreated()
    {
        StartCoroutine(UpdateLightingAfterChunksCreatedCoroutine());

    }

    private IEnumerator UpdateLightingAfterChunksCreatedCoroutine()
    {
        for (int i = 0; i < smoothIteration; i++)
        {
            int coroutinesCompleted = 0; // Counter to track completed coroutines
            int coroutinesToComplete = WorldGenerator.ActiveChunks.Count; // Total coroutines to wait for

            // Start all coroutines for this iteration
            foreach (var activeChunk in WorldGenerator.ActiveChunks)
            {
                int chunkCoord = activeChunk.Key;
                Vector2Int pos = new Vector2Int(chunkCoord, 0);
                StartCoroutine(GeneratorInstance.UpdateChunkLight(pos, () => coroutinesCompleted++));
            }

            // Wait for all coroutines to complete
            yield return new WaitUntil(() => coroutinesCompleted == coroutinesToComplete);
        }

        // After all iterations are complete, proceed with applying light to all chunks
        foreach (var activeChunk in WorldGenerator.ActiveChunks)
        {
            int chunkCoord = activeChunk.Key;
            Vector2Int pos = new Vector2Int(chunkCoord, 0);
            if (WorldGenerator.WorldLightData.ContainsKey(pos) && WorldGenerator.WorldLightTexture.ContainsKey(pos))
            {
                StartCoroutine(GeneratorInstance.ApplyLightToChunk(WorldGenerator.WorldLightTexture[pos], WorldGenerator.WorldLightData[pos], pos));
            }
        }
    }


    void InitializeTerrainAtDefaultPosition()
    {
        // Start by generating chunk 0
        if (!WorldGenerator.ActiveChunks.ContainsKey(0))
        {
            GenerateChunkWithCallback(0);
        }
        // Assuming chunk 0 as the starting point
        int startChunk = 0;
        for (int x = startChunk - RenderDistance; x <= startChunk + RenderDistance; x++)
        {
            // Skip chunk 0 since it's already been handled
            if (x == 0) continue;

            int chunkCoord = x;
            if (!WorldGenerator.ActiveChunks.ContainsKey(chunkCoord))
            {
                GenerateChunkWithCallback(chunkCoord);
            }
        }
    }
    private IEnumerator CreateChunkWithCallback(int chunkCoord, Action callback)
    {
        yield return StartCoroutine(GeneratorInstance.CreateChunk(chunkCoord));
        callback?.Invoke();
    }

    void GenerateChunkWithCallback(int chunkCoord)
    {
        StartCoroutine(GeneratorInstance.CreateChunk(chunkCoord, () =>
        {
            if (chunkCoord == 0)
            {
                GeneratorInstance.AddCoreArchitectureToChunk(chunkCoord);
            }
        }));
    }

}
