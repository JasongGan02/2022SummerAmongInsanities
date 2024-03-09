using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LightGenerator : MonoBehaviour
{
    [Tooltip("between 0 & 15")][SerializeField] private float sunlightBrightness = 15f;
    [SerializeField] private int iterations = 7;
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
    }

    public class GenData
    {
        public System.Action<float[,]> OnComplete;
        public Vector2Int GenerationPoint;
        public bool isRefreshing = false;
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
                yield return StartCoroutine(GenerateLightData(gen.GenerationPoint, gen.OnComplete, gen.isRefreshing));
            }
            else
            {
                yield return null; // Wait until there's data to generate
            }
        }
    }

    public IEnumerator GenerateLightData(Vector2Int offset, System.Action<float[,]> callback, bool isRefreshing = false)
    {
        Vector2Int ChunkSize = WorldGenerator.ChunkSize;
        TileObject[,,] chunkData = WorldGenerator.WorldData[offset];
        float[,] lightData = new float[ChunkSize.x, ChunkSize.y];

        Task t = Task.Factory.StartNew(delegate
        {
            float[] leftEdgeData = GetEdgeLightData(offset, getLeftEdge: true);
            float[] rightEdgeData = GetEdgeLightData(offset, getLeftEdge: false);
            
            for (int i = 0; i < iterations; i++)
            {
                for (int x = 0; x < ChunkSize.x; x++)
                {
                    float lightLevel = sunlightBrightness;
                    for (int y = ChunkSize.y - 1; y >= 0; y--)
                    {
                        bool isLit = false;
                        TileObject curTile = null;
                        for (int z = 0; z < chunkData.GetLength(2); z++)
                        {
                            if (chunkData[x, y, z] is null) continue;
                            if (chunkData[x, y, z].IsLit)
                            {
                                isLit = true;
                                curTile = chunkData[x, y, z];
                            }
                        }
                      
                        if (chunkData[x, y, 0] is null && chunkData[x, y, 1] is null && chunkData[x, y, 2] is null) //if illuminate block
                            lightLevel = sunlightBrightness; //sky light
                        else if (isLit)
                        {
                            lightLevel = curTile.LightIntensity; //tile's own lightIntensity
                        }
                        else
                        {
                            //find brightest neighbour
                            int nx1 = Mathf.Clamp(x - 1, 0, ChunkSize.x - 1);
                            int nx2 = Mathf.Clamp(x + 1, 0, ChunkSize.x - 1);
                            int ny1 = Mathf.Clamp(y - 1, 0, ChunkSize.y - 1);
                            int ny2 = Mathf.Clamp(y + 1, 0, ChunkSize.y - 1);
                            
                            //cross-chunks egde cases
                            float leftEdgeLight = x == 0 && leftEdgeData != null ? leftEdgeData[y] : 0;
                            float rightEdgeLight = x == ChunkSize.x - 1 && rightEdgeData != null ? rightEdgeData[y] : 0;
                            
                            lightLevel = Mathf.Max(
                                lightData[nx1, y],
                                lightData[nx2, y],
                                lightData[x, ny1],
                                lightData[x, ny2],
                                leftEdgeLight,
                                rightEdgeLight);
                                
                            if (chunkData[x, y, 1] is null)
                                lightLevel -= 0.75f;
                            else
                                lightLevel -= 2.5f;
                        }

                        lightData[x, y] = lightLevel;
                    }
                }

                //reverse calculation to remove artifacts
                for (int x = ChunkSize.x - 1; x >= 0; x--)
                {
                    float lightLevel;
                    for (int y = 0; y < ChunkSize.y; y++)
                    {
                        //find brightest neighbour
                        int nx1 = Mathf.Clamp(x - 1, 0, ChunkSize.x - 1);
                        int nx2 = Mathf.Clamp(x + 1, 0, ChunkSize.x - 1);
                        int ny1 = Mathf.Clamp(y - 1, 0, ChunkSize.y - 1);
                        int ny2 = Mathf.Clamp(y + 1, 0, ChunkSize.y - 1);

                        //cross-chunks egde cases
                        float leftEdgeLight = x == 0 && leftEdgeData != null ? leftEdgeData[y] : 0;
                        float rightEdgeLight = x == ChunkSize.x - 1 && rightEdgeData != null ? rightEdgeData[y] : 0;
                            
                        lightLevel = Mathf.Max(
                            lightData[nx1, y],
                            lightData[nx2, y],
                            lightData[x, ny1],
                            lightData[x, ny2],
                            leftEdgeLight,
                            rightEdgeLight);

                        if (chunkData[x, y, 1] is null) //wall should also be considered as air but they are not lit wall is in 0, others'z are > 0. Any z bigger than 1 would be considered as air block
                            lightLevel -= 0.75f;
                        else
                            lightLevel -= 2.5f;

                        lightData[x, y] = lightLevel;
                    }
                }
            }

            for (int x = 0; x < ChunkSize.x; x++)
            {
                for (int y = 0; y < ChunkSize.y; y++)
                {
                    float adjustedValue = 1 - (lightData[x, y] / sunlightBrightness);
                    lightData[x, y] = adjustedValue <= 0.051 ? 0 : adjustedValue;
                }
            }

        });

        yield return new WaitUntil(() => {
            return t.IsCompleted || t.IsCanceled;
        });

        if (t.Exception != null)
            Debug.LogError(t.Exception);

        
        WorldGenerator.WorldLightData[offset] = lightData;
        if (lightData != null)
            callback(lightData);

        if (!isRefreshing)
        {
            Vector2Int leftChunkOffset = new Vector2Int(offset.x - 1, 0);
            if (WorldGenerator.WorldLightData.ContainsKey(leftChunkOffset))
            {
                worldGenerator.RefreshChunkLight(leftChunkOffset, true);
            }
            
            Vector2Int rightChunkOffset = new Vector2Int(offset.x + 1, 0);
            if (WorldGenerator.WorldLightData.ContainsKey(rightChunkOffset))
            {
                worldGenerator.RefreshChunkLight(rightChunkOffset, true);
            }
        }
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

            int columnIndex = getLeftEdge ? chunkLightData.GetLength(0) - 1 : 0;
            for (int y = 0; y < height; y++)
            {
                // Convert adjusted light data back to raw light data
                float adjustedValue = chunkLightData[columnIndex, y];
                float originalValue = sunlightBrightness * (1 - adjustedValue);
                edgeData[y] = originalValue;
            }

            return edgeData;
        }
        
    }

}

