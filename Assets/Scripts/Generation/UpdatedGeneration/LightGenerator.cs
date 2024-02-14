using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using System.Diagnostics;
public class LightGenerator : MonoBehaviour
{
    //Process the data in multithreading here, and return the OnComplete to apply to the Material 
    public class GenData
    {
        public System.Action<float[,]> OnComplete;
        public Vector2Int GenerationPoint;
    }

    public LightGenerator(LightOverlayManager lightOverlayManager)
    {
        this.lightOverlayManager = lightOverlayManager;
        DataToGenerate = new Queue<GenData>();
        lightOverlayManager.StartCoroutine(DataGenLoop());
    }
    public void QueueDataToGenerate(GenData data)
    {
        DataToGenerate.Enqueue(data);
    }

    public IEnumerator DataGenLoop()
    {
        while (Terminate == false)
        {
            if (DataToGenerate.Count > 0)
            {
                GenData gen = DataToGenerate.Dequeue();
                yield return lightOverlayManager.StartCoroutine(GenerateLightData(gen.GenerationPoint, gen.OnComplete));
            }

            yield return null;
        }
    }

    [Tooltip("between 0 & 15")][SerializeField] private float sunlightBrightness = 15f;
    [SerializeField] private int iterations = 2;
    private Queue<GenData> DataToGenerate;
    private LightOverlayManager lightOverlayManager;
    public bool Terminate;

    public IEnumerator GenerateLightData(Vector2Int offset, System.Action<float[,]> callback)
    {
        int minX = WorldGenerator.ActiveChunks.Keys.Min();
        int maxX = WorldGenerator.ActiveChunks.Keys.Max();

        int sizeX = (maxX - minX + 1) * WorldGenerator.ChunkSize.x;
        Vector2Int ChunkSize = WorldGenerator.ChunkSize;
        ChunkSize.x = sizeX;

        float[,] lightData = new float[ChunkSize.x, ChunkSize.y];
        
        Task t = Task.Factory.StartNew(delegate
        {
            //Aggregate
            int[,] aggregatedWorldData  = new int[ChunkSize.x, ChunkSize.y];
            foreach (var chunkEntry in WorldGenerator.ActiveChunks)
            {
                Vector2Int chunkOffset = new Vector2Int(chunkEntry.Key, 0);
                int[,] chunkData  = WorldGenerator.WorldData[chunkOffset];

                // Calculate the start position in the aggregated array for this chunk
                int startX = (chunkOffset.x - minX) * WorldGenerator.ChunkSize.x;

                // Copy chunk data into the aggregated world data array
                for (int x = 0; x < WorldGenerator.ChunkSize.x; x++)
                {
                    for (int y = 0; y < WorldGenerator.ChunkSize.y; y++)
                    {
                        aggregatedWorldData[startX + x, y] = chunkData[x, y];
                    }
                }
            }


            for (int i = 0; i < iterations; i++)
            {
                for (int x = 0; x < ChunkSize.x; x++)
                {
                    float lightLevel = sunlightBrightness;
                    for (int y = ChunkSize.y - 1; y >= 0; y--)
                    {
                        TileObject tileObject = TileObjectRegistry.GetTileObjectByID(aggregatedWorldData[x, y]);
                        if ((tileObject != null && tileObject.IsLit) || aggregatedWorldData[x, y] == 0) //if illuminate block

                            lightLevel = sunlightBrightness;
                        else
                        {
                            //find brightest neighbour
                            int nx1 = Mathf.Clamp(x - 1, 0, ChunkSize.x - 1);
                            int nx2 = Mathf.Clamp(x + 1, 0, ChunkSize.x - 1);
                            int ny1 = Mathf.Clamp(y - 1, 0, ChunkSize.y - 1);
                            int ny2 = Mathf.Clamp(y + 1, 0, ChunkSize.y - 1);

                            lightLevel = Mathf.Max(
                                lightData[nx1, y],
                                lightData[nx2, y],
                                lightData[x, ny1],
                                lightData[x, ny2]);

                            if (aggregatedWorldData[x, y] <= 0)
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

                        lightLevel = Mathf.Max(
                            lightData[nx1, y],
                            lightData[nx2, y],
                            lightData[x, ny1],
                            lightData[x, ny2]);

                        if (aggregatedWorldData[x, y] <= 0) //wall should also be considered as air but they are not lit
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
                    lightData[x, y] = adjustedValue <= 0.05 ? 0 : adjustedValue;
                }
            }

        });

        yield return new WaitUntil(() => {
            return t.IsCompleted || t.IsCanceled;
        });

        if (t.Exception != null)
            UnityEngine.Debug.LogError(t.Exception);

        WorldGenerator.ActiveWorldLightData = lightData;
        callback(lightData);
    }

}

