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

        // Optionally start the DataGenLoop here or allow other scripts to control when it starts
        StartCoroutine(DataGenLoop());
    }

    public class GenData
    {
        public System.Action<float[,]> OnComplete;
        public Vector2Int GenerationPoint;
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
                yield return StartCoroutine(GenerateLightData(gen.GenerationPoint, gen.OnComplete));
            }
            else
            {
                yield return null; // Wait until there's data to generate
            }
        }
    }

    public IEnumerator GenerateLightData(Vector2Int offset, System.Action<float[,]> callback)
    {
        Vector2Int ChunkSize = WorldGenerator.ChunkSize;
        TileObject[,,] chunkData = WorldGenerator.WorldData[offset];
        float[,] lightData = new float[ChunkSize.x, ChunkSize.y];

        Task t = Task.Factory.StartNew(delegate
        {
            if (!WorldGenerator.WorldLightData.TryGetValue(offset, out lightData))
            {
                lightData = new float[ChunkSize.x, ChunkSize.y];
            }
            else
            {
                float[] leftEdgeData = GetEdgeLightData(offset, getLeftEdge: true);
                float[] rightEdgeData = GetEdgeLightData(offset, getLeftEdge: false);

                if (leftEdgeData != null || rightEdgeData != null)
                {
                    if (!saved)
                    {
                        string path = "C:\\Users\\34061\\AppData\\LocalLow\\DefaultCompany\\AmongInsanities " + "/lightData1.txt";
                        SaveLightDataToFile(lightData, path);
                        lightData = AdjustChunkLightingWithEdgeData(offset, leftEdgeData, rightEdgeData);
                        path = "C:\\Users\\34061\\AppData\\LocalLow\\DefaultCompany\\AmongInsanities " + "/lightData2.txt";
                        SaveLightDataToFile(lightData, path);
                        saved = true;
                    }
                    else
                    {
                        lightData = AdjustChunkLightingWithEdgeData(offset, leftEdgeData, rightEdgeData);
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
                        bool isLit = false;
                        for (int z = 0; z < chunkData.GetLength(2); z++)
                        {
                            if (chunkData[x, y, z] is null) continue;
                            if (chunkData[x, y, z].IsLit) isLit = true;
                        }
                      
                        if (isLit || (chunkData[x, y, 1] is null &&
                                chunkData[x, y, 2] is null)) //if illuminate block
                            //TODO: litObject has a parameter of their brightness level <= sunlightBrightness
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

                        lightLevel = Mathf.Max(
                            lightData[nx1, y],
                            lightData[nx2, y],
                            lightData[x, ny1],
                            lightData[x, ny2]);

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

        Debug.Log(WorldGenerator.WorldLightData.ContainsKey(offset));
        WorldGenerator.WorldLightData[offset] = lightData;
        if (lightData != null)
            callback(lightData);
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
                edgeData[y] = chunkLightData[columnIndex, y];
            }

            return edgeData;
        }
        
    }

    float[,] AdjustChunkLightingWithEdgeData(Vector2Int chunkPosition, float[] leftEdgeData, float[] rightEdgeData)
    {
        float[,] lightData = WorldGenerator.WorldLightData[chunkPosition];
        Vector2Int ChunkSize = WorldGenerator.ChunkSize;
        TileObject[,,] chunkData = WorldGenerator.WorldData[chunkPosition];
       
        // Adjust the left edge using the right edge data of the left neighbor
        if (leftEdgeData != null)
        {
            for (int y = 0; y < ChunkSize.y; y++)
            {
                lightData[0, y] = Mathf.Max(lightData[0, y], leftEdgeData[y] - (chunkData[0, y, 1] is null ? 0.75f : 2.5f));
            }
        }

        // Adjust the right edge using the left edge data of the right neighbor
        if (rightEdgeData != null)
        {
            for (int y = 0; y < ChunkSize.y; y++)
            {
                lightData[ChunkSize.x - 1, y] = Mathf.Max(lightData[ChunkSize.x - 1, y], rightEdgeData[y] - (chunkData[0, y, 1] is null ? 0.75f : 2.5f)); 
            }
        }

        return lightData;

    }
    public void SaveLightDataToFile(float[,] lightData, string filePath)
    {
        StringBuilder sb = new StringBuilder();
        int width = lightData.GetLength(0);
        int height = lightData.GetLength(1);

        // Iterate over each element in the 2D array and append it to the StringBuilder
        for (int y = height - 1; y >= 0; y--) // Assume y is the second dimension
        {
            for (int x = 0; x < width; x++)
            {
                // Append each light data value followed by a space or other delimiter
                sb.Append(lightData[x, y].ToString("F2")); // "F2" formats the float to 2 decimal places
                if (x < width - 1) sb.Append(" "); // Add a space delimiter between values in the same row
            }
            // After each row, append a newline character to start a new row
            if (y < height - 1) sb.AppendLine();
        }

        // Write the StringBuilder content to a file
        File.WriteAllText(filePath, sb.ToString());

        Debug.Log($"Light data saved to {filePath}");
    }


}

