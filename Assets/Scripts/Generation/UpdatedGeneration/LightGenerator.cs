using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LightGenerator : MonoBehaviour
{
    //Process the data in multithreading here, and return the OnComplete to apply to the Material 
    public class GenData
    {
        public System.Action<float[,]> OnComplete;
        public Vector2Int GenerationPoint;
    }

    public LightGenerator(WorldGenerator worldGen)
    {
        GeneratorInstance = worldGen;
        DataToGenerate = new Queue<GenData>();
        worldGen.StartCoroutine(DataGenLoop());
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
                yield return GeneratorInstance.StartCoroutine(GenerateLightData(gen.GenerationPoint, gen.OnComplete));
            }

            yield return null;
        }
    }

    [Tooltip("between 0 & 15")][SerializeField] private float sunlightBrightness = 15f;
    [SerializeField] private int iterations = 7;
    private Queue<GenData> DataToGenerate;
    private WorldGenerator GeneratorInstance;
    public bool Terminate;

    public IEnumerator GenerateLightData(Vector2Int offset, System.Action<float[,]> callback)
    {
        Vector2Int ChunkSize = WorldGenerator.ChunkSize;
        float[,] lightData = new float[ChunkSize.x, ChunkSize.y];
        int[,] chunkData = WorldGenerator.WorldData[offset];
        Task t = Task.Factory.StartNew(delegate
        {
            for (int i = 0; i < iterations; i++)
            {
                for (int x = 0; x < ChunkSize.x; x++)
                {
                    float lightLevel = sunlightBrightness;
                    for (int y = ChunkSize.y - 1; y >= 0; y--)
                    {
                        TileObject tileObject = TileObjectRegistry.GetTileObjectByID(chunkData[x, y]);
                        if ((tileObject != null && tileObject.IsLit) || chunkData[x, y] == 0) //if illuminate block

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

                            if (chunkData[x, y] <= 0)
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

                        if (chunkData[x, y] <= 0) //wall should also be considered as air but they are not lit
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
            Debug.LogError(t.Exception);

        WorldGenerator.WorldLightData.Add(offset, lightData);
        callback(lightData);
    }

}

