using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrainGenerator : MonoBehaviour
{

    [SerializeField] private Transform Player;
    public static int RenderDistance = 3;
    private LightOverlayManager lightOverlayManager;
    private WorldGenerator GeneratorInstance;
    private List<int> CoordsToRemove;
    private int lastFrameChunkX = 0;
    // Start is called before the first frame update
    void Start()
    {
        GeneratorInstance = GetComponent<WorldGenerator>();
        lightOverlayManager = GetComponent<LightOverlayManager>();
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
        //Refresh Lightmap if a new chunk is added
        if (lastFrameChunkX == 0 || lastFrameChunkX != plrChunkX)
            lightOverlayManager.UpdateLightOverlayBoundsAndPosition();
        lastFrameChunkX = plrChunkX;
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
