using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrainGenerator : MonoBehaviour
{

    [SerializeField] private Transform Player;
    [SerializeField] private int RenderDistance;
    private WorldGenerator GeneratorInstance;
    private List<int> CoordsToRemove;

    // Start is called before the first frame update
    void Start()
    {
        GeneratorInstance = GetComponent<WorldGenerator>();
        CoordsToRemove = new List<int>();
        if (Player == null)
        {
            //InitializeTerrainAtDefaultPosition();
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
        int plrChunkX = (int)Player.position.x / WorldGenerator.ChunkSize.x;
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
                StartCoroutine(GeneratorInstance.CreateChunk(chunkCoord));
            }

            CoordsToRemove.Remove(chunkCoord);
           
        }

        foreach (int coord in CoordsToRemove)
        {
            GameObject chunkToDisable = WorldGenerator.ActiveChunks[coord];
            WorldGenerator.ActiveChunks.Remove(coord);
            chunkToDisable.SetActive(false);
        }
    }

    void InitializeTerrainAtDefaultPosition()
    {
        // Assuming chunk 0 as the starting point
        int startChunk = 0;
        for (int x = startChunk - RenderDistance; x <= startChunk + RenderDistance; x++)
        {
            int chunkCoord = x;
            if (!WorldGenerator.ActiveChunks.ContainsKey(chunkCoord))
            {
                StartCoroutine(GeneratorInstance.CreateChunk(chunkCoord));
            }
        }
    }
}
