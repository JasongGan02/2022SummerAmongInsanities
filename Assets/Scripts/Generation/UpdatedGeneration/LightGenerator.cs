using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LightGenerator : MonoBehaviour
{
    [Tooltip("between 0 & 15")][SerializeField] private float sunlightBrightness = 15f;
    [SerializeField] private int iterations = 4;
    private Queue<GenData> DataToGenerate = new Queue<GenData>();
    public bool Terminate;
    bool saved = false;
    // Singleton instance
    public static LightGenerator Instance { get; private set; }
    private WorldGenerator worldGenerator;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Optional: Keep instance alive across scenes
        }

        worldGenerator = WorldGenerator.Instance;
        // Optionally start the DataGenLoop here or allow other scripts to control when it starts
        StartCoroutine(DataGenLoop());
        StartCoroutine(PeriodicLightUpdateCoroutine());
    }

    private void OnDisable()
    {
         StopAllCoroutines();
    }

    public class GenData
    {
        public System.Action<float[,]> OnComplete;
        public Vector2Int GenerationPoint;
        public bool UpdateNeighbors = false;
    }

    public void QueueDataToGenerate(GenData data)
    {
        DataToGenerate.Enqueue(data);
    }

    public IEnumerator DataGenLoop()
    {
        while (!Terminate)
        {
            if (DataToGenerate.Count > 0)
            {
                GenData gen = DataToGenerate.Dequeue();
                yield return StartCoroutine(GenerateLightData(gen.GenerationPoint, gen.OnComplete, gen.UpdateNeighbors));
            }
            else
            {
                yield return null; // Wait until there's data to generate
            }
        }
    }

    public IEnumerator GenerateLightData(Vector2Int offset, System.Action<float[,]> callback,
        bool updateNeighbors = false, bool getEdgeData = true)
    {
        Vector2Int ChunkSize = WorldGenerator.ChunkSize;
        if (!WorldGenerator.WorldData.TryGetValue(offset, out var chunkData))
            yield break;
        float[,] lightData = new float[ChunkSize.x, ChunkSize.y];

        Task t = Task.Factory.StartNew(delegate
        {
            float[] leftEdgeData = getEdgeData ? GetEdgeLightData(offset, getLeftEdge: true) : new float[ChunkSize.y];
            float[] rightEdgeData = getEdgeData ? GetEdgeLightData(offset, getLeftEdge: false) : new float[ChunkSize.y];


            for (int i = 0; i < iterations; i++)
            {
                for (int x = 0; x < ChunkSize.x; x++)
                {
                    float lightLevel = sunlightBrightness;
                    for (int y = ChunkSize.y - 1; y >= 0; y--)
                    {
                        float maxNeighborLightLevel = 0; // Initialize the maximum neighbor light level
                        TileObject curTile = null;
                        for (int z = 1; z < chunkData.GetLength(2); z++)
                        {
                            if (chunkData[x, y, z] != null && chunkData[x, y, z].IsLit)
                            {
                                curTile = chunkData[x, y, z];
                                break; // Found the lit tile, no need to continue the loop
                            }
                        }
                        
                        // Calculate maximum light level from neighbors
                        int nx1 = Mathf.Clamp(x - 1, 0, ChunkSize.x - 1);
                        int nx2 = Mathf.Clamp(x + 1, 0, ChunkSize.x - 1);
                        int ny1 = Mathf.Clamp(y - 1, 0, ChunkSize.y - 1);
                        int ny2 = Mathf.Clamp(y + 1, 0, ChunkSize.y - 1);
                        float leftEdgeLight = x == 0 && leftEdgeData != null ? leftEdgeData[y] : 0;
                        float rightEdgeLight = x == ChunkSize.x - 1 && rightEdgeData != null ? rightEdgeData[y] : 0;
                        maxNeighborLightLevel = Mathf.Max(
                            lightData[nx1, y],
                            lightData[nx2, y],
                            lightData[x, ny1],
                            lightData[x, ny2],
                            leftEdgeLight,
                            rightEdgeLight);
                        
                        //Light Attenuation
                        if (chunkData[x, y, 1] != null)
                        {
                            maxNeighborLightLevel -= 2.5f; // Block attenuation
                        }
                        else
                        {
                            maxNeighborLightLevel -= 0.75f; // Air attenuation
                        }
                        
                        maxNeighborLightLevel = Mathf.Max(maxNeighborLightLevel, 0); //make sure it doesn't go below 0
                        
                        //Air Block
                        if (chunkData[x, y, 0] is null && chunkData[x, y, 1] is null &&
                            chunkData[x, y, 2] is null)
                        {
                            lightLevel = Mathf.Max(sunlightBrightness, maxNeighborLightLevel);
                        } 
                        else if (curTile != null) //the block itself is lit
                        {
                            // Compare tile's own light intensity, propagated light
                            lightLevel = Mathf.Max(curTile.LightIntensity, maxNeighborLightLevel);
                        }
                        else
                        {
                            // For non-lit tiles, the light level is the maximum of neighbor
                            lightLevel = maxNeighborLightLevel;
                        }

                        lightData[x, y] = lightLevel;
                    }
                }

                //reverse calculation to remove artifacts
                for (int x = ChunkSize.x - 1; x >= 0; x--)
                {
                    float lightLevel = sunlightBrightness;
                    for (int y = ChunkSize.y - 1; y >= 0; y--)
                    {
                        float maxNeighborLightLevel = 0; // Initialize the maximum neighbor light level
                        TileObject curTile = null;
                        for (int z = 1; z < chunkData.GetLength(2); z++)
                        {
                            if (chunkData[x, y, z] != null && chunkData[x, y, z].IsLit)
                            {
                                curTile = chunkData[x, y, z];
                                break; // Found the lit tile, no need to continue the loop
                            }
                        }
                        
                        // Calculate maximum light level from neighbors
                        int nx1 = Mathf.Clamp(x - 1, 0, ChunkSize.x - 1);
                        int nx2 = Mathf.Clamp(x + 1, 0, ChunkSize.x - 1);
                        int ny1 = Mathf.Clamp(y - 1, 0, ChunkSize.y - 1);
                        int ny2 = Mathf.Clamp(y + 1, 0, ChunkSize.y - 1);
                        float leftEdgeLight = x == 0 && leftEdgeData != null ? leftEdgeData[y] : 0;
                        float rightEdgeLight = x == ChunkSize.x - 1 && rightEdgeData != null ? rightEdgeData[y] : 0;
                        maxNeighborLightLevel = Mathf.Max(
                            lightData[nx1, y],
                            lightData[nx2, y],
                            lightData[x, ny1],
                            lightData[x, ny2],
                            leftEdgeLight,
                            rightEdgeLight);
                        
                        //Light Attenuation
                        if (chunkData[x, y, 1] != null)
                        {
                            maxNeighborLightLevel -= 2.5f; // Block attenuation
                        }
                        else
                        {
                            maxNeighborLightLevel -= 0.75f; // Air attenuation
                        }
                        
                        maxNeighborLightLevel = Mathf.Max(maxNeighborLightLevel, 0); //make sure it doesn't go below 0
                        
                        //Air Block
                        if (chunkData[x, y, 0] is null && chunkData[x, y, 1] is null &&
                            chunkData[x, y, 2] is null)
                        {
                            lightLevel = Mathf.Max(sunlightBrightness, maxNeighborLightLevel);
                        } 
                        else if (curTile != null) //the block itself is lit
                        {
                            // Compare tile's own light intensity, propagated light
                            lightLevel = Mathf.Max(curTile.LightIntensity, maxNeighborLightLevel);
                        }
                        else
                        {
                            // For non-lit tiles, the light level is the maximum of neighbor
                            lightLevel = maxNeighborLightLevel;
                        }

                        lightData[x, y] = lightLevel;
                    }
                }
            }

            for (int x = 0; x < ChunkSize.x; x++)
            {
                for (int y = 0; y < ChunkSize.y; y++)
                {
                    float adjustedValue = 1 - (lightData[x, y] / 15);
                    lightData[x, y] = adjustedValue <= 0.051 ? 0 : adjustedValue;
                }
            }

        }).ContinueWith(t =>
        {


            // Ensuring callback and neighbor updates are handled on the Unity main thread
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                if (t.Exception != null)
                    Debug.LogError(t.Exception);
                else
                {
                    WorldGenerator.WorldLightData[offset] = lightData;
                    callback?.Invoke(lightData);

                    if (updateNeighbors)
                    {
                        Vector2Int leftChunkOffset = new Vector2Int(offset.x - 1, 0);
                        if (WorldGenerator.WorldLightData.ContainsKey(leftChunkOffset))
                        {
                            worldGenerator.RefreshChunkLight(leftChunkOffset);
                        }

                        Vector2Int rightChunkOffset = new Vector2Int(offset.x + 1, 0);
                        if (WorldGenerator.WorldLightData.ContainsKey(rightChunkOffset))
                        {
                            worldGenerator.RefreshChunkLight(rightChunkOffset);
                        }
                    }
                }
            });

        });
        yield return new WaitUntil(() => t.IsCompleted || t.IsCanceled);
    }

    private float[] GetEdgeLightData(Vector2Int chunkPosition, bool getLeftEdge)
    {

        Vector2Int targetChunkPosition = new Vector2Int(chunkPosition.x + (getLeftEdge ? -1 : 1), 0);
        if (!WorldGenerator.WorldLightData.TryGetValue(targetChunkPosition, out var chunkLightData))
        {
            return null;

        }
        else
        {
            int height = chunkLightData.GetLength(1);
            float[] edgeData = new float[height];
            TileObject[,,] chunkData = WorldGenerator.WorldData[targetChunkPosition];
            int columnIndex = getLeftEdge ? chunkLightData.GetLength(0) - 1 : 0;
            float lightLevel;
            for (int y = 0; y < height; y++)
            {
                float maxNeighborLightLevel = 0; // Initialize the maximum neighbor light level
                TileObject curTile = null;
                for (int z = 1; z < chunkData.GetLength(2); z++)
                {
                    if (chunkData[columnIndex, y, z] != null && chunkData[columnIndex, y, z].IsLit)
                    {
                        curTile = chunkData[columnIndex, y, z];
                        break; // Found the lit tile, no need to continue the loop
                    }
                }
                
                // Calculate maximum light level from neighbors
                int ny1 = Mathf.Clamp(y - 1, 0, WorldGenerator.ChunkSize.y - 1);
                int ny2 = Mathf.Clamp(y + 1, 0, WorldGenerator.ChunkSize.y - 1);

                maxNeighborLightLevel = Mathf.Min(
                    chunkLightData[columnIndex, ny1],
                    chunkLightData[columnIndex, ny2]);

                maxNeighborLightLevel = 15 * (1 - maxNeighborLightLevel);
                
                //Light Attenuation
                if (chunkData[columnIndex, y, 1] != null)
                {
                    maxNeighborLightLevel -= 2.5f; // Block attenuation
                }
                else
                {
                    maxNeighborLightLevel -= 0.75f; // Air attenuation
                }
                
                maxNeighborLightLevel = Mathf.Max(maxNeighborLightLevel, 0); //make sure it doesn't go below 0
                
                //Air Block
                if (chunkData[columnIndex, y, 0] is null && chunkData[columnIndex, y, 1] is null &&
                    chunkData[columnIndex, y, 2] is null)
                {
                    lightLevel = Mathf.Max(sunlightBrightness, maxNeighborLightLevel);
                } 
                else if (curTile != null) //the block itself is lit
                {
                    // Compare tile's own light intensity, propagated light
                    lightLevel = Mathf.Max(curTile.LightIntensity, maxNeighborLightLevel);
                }
                else
                {
                    // For non-lit tiles, the light level is the maximum of neighbor
                    lightLevel = maxNeighborLightLevel;
                }

                edgeData[y] = lightLevel;
            }

            return edgeData;
        }
        
    }
    
    private IEnumerator PeriodicLightUpdateCoroutine()
    {
        while (!Terminate) // Use the existing termination condition
        {
            yield return new WaitForSeconds(5f); // Wait for 5 seconds

            RefreshAllActiveChunks(); // Call the method to refresh all active chunks' light
        }
    }

    public void UpdateSunlightBrightness(float newSunlightBrightness)
    {
        sunlightBrightness = newSunlightBrightness;
        RefreshAllActiveChunks();
    }

    private void RefreshAllActiveChunks()
    {
        StartCoroutine(RefreshAllActiveChunksCoroutine());
    }

    private static int ID;
    private IEnumerator RefreshAllActiveChunksCoroutine()
    {
        yield return new WaitForEndOfFrame();
        var chunksToRefresh = new List<int>(WorldGenerator.ActiveChunks.Keys);
        int chunksCompleted = 0;
        
        Action<float[,]> onChunkUpdateComplete = (lightData) =>
        {
            chunksCompleted++;
            // Here, you might want to apply the updated light data to the chunk
            // For example, by updating the texture or mesh based on the new light data
            // Ensure this action is thread-safe or executed on the main thread if necessary
        };
        
        foreach (var chunk in chunksToRefresh)
        {
            float[,] lightDataToApply = null;
            Vector2Int chunkPosition = new Vector2Int(chunk, 0); // Adjust as necessary
            QueueDataToGenerate(new LightGenerator.GenData
            {
                GenerationPoint = chunkPosition,
                OnComplete = onChunkUpdateComplete,
                UpdateNeighbors =  true
            });
        }
        
        yield return new WaitUntil(() => chunksCompleted >= chunksToRefresh.Count);
        
        foreach (var chunk in chunksToRefresh)
        {
            Vector2Int chunkPosition = new Vector2Int(chunk, 0); // Adjust as necessary
            if (WorldGenerator.WorldLightTexture.ContainsKey(chunkPosition) && WorldGenerator.WorldLightData.ContainsKey(chunkPosition))
                worldGenerator.ApplyLightToChunk(WorldGenerator.WorldLightTexture[chunkPosition], WorldGenerator.WorldLightData[chunkPosition]);
        }
    }



}

