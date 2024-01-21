using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour, IDataPersistence
{
    public static Dictionary<Vector2Int, int[,]> WorldData;
    public static Dictionary<Vector2Int, float[,]> WorldLightData;
    public static Dictionary<int, GameObject> ActiveChunks;
    public static Dictionary<int, GameObject> TotalChunks;
    public static Dictionary<int, int[,]> AdditiveWorldData;
    public static readonly Vector2Int ChunkSize = new Vector2Int(16, 256);
    public float seed;
    public TerrainSettings[] settings;
    public TileObjectRegistry tileObjectRegistry;

    [SerializeField] private TowerObject coreArchitectureSO;
    private DataGenerator dataCreator;
    private LightGenerator lightGenerator;

    [Header("Lighting")]
    //Light
    public Material lightShader;
    [SerializeField] private GameObject lightOverlayPrefab;
    // Start is called before the first frame update
    void Awake()
    {
        if (tileObjectRegistry != null)
        {
            tileObjectRegistry.Initialize();
        }
        else
        {
            Debug.LogError("TileObjectRegistry is not assigned in WorldGenerator");
        }
        WorldData = new Dictionary<Vector2Int, int[,]>();
        WorldLightData = new Dictionary<Vector2Int, float[,]>();
        ActiveChunks = new Dictionary<int, GameObject>();
        TotalChunks = new Dictionary<int, GameObject>();
        dataCreator = new DataGenerator(this, settings, GetComponent<StructureGenerator>());
        lightGenerator = new LightGenerator(this);


        RecalculateSeed();
    }
    private void RecalculateSeed() { if (seed == 0) seed = UnityEngine.Random.Range(-10000, 10000); }

    public IEnumerator CreateChunk(int ChunkCoord, Action onChunkCreated = null)
    {
        if (TotalChunks.ContainsKey(ChunkCoord))
        {
            TotalChunks[ChunkCoord].SetActive(true);
            ActiveChunks.Add(ChunkCoord, TotalChunks[ChunkCoord]);
            yield break;
        }
        Vector2Int pos = new Vector2Int(ChunkCoord, 0);
        string chunkName = $"Chunk {ChunkCoord}";

        GameObject newChunk = new GameObject(chunkName);
        
        newChunk.transform.position = new Vector2(ChunkCoord * ChunkSize.x, 0f);
        newChunk.transform.SetParent(transform, true);
        ActiveChunks.Add(ChunkCoord, newChunk);

        int[,] dataToApply = WorldData.ContainsKey(pos) ? WorldData[pos] : null;

        if (dataToApply == null)
        {
            dataCreator.QueueDataToGenerate(new DataGenerator.GenData
            {
                GenerationPoint = pos,
                OnComplete = x => dataToApply = x
            });

            yield return new WaitUntil(() => dataToApply != null);
        }

        if (!TotalChunks.ContainsKey(ChunkCoord))
            TotalChunks.Add(ChunkCoord, newChunk);

        StartCoroutine(DrawChunk(dataToApply, pos, () =>
        {
            onChunkCreated?.Invoke(); // Call the callback after drawing is complete
        }));

        //Process Light; Propogate Light Map Data
        

        float[,] lightDataToApply = WorldLightData.ContainsKey(pos) ? WorldLightData[pos] : null;
        
        if (lightDataToApply == null)
        {
            lightGenerator.QueueDataToGenerate(new LightGenerator.GenData
            {
                GenerationPoint = pos,
                OnComplete = x => lightDataToApply = x
            });

            yield return new WaitUntil(() => lightDataToApply != null);
        }
        string lightOverlayName = $"Light Overlay {ChunkCoord}";
        GameObject lightMapOverlay = Instantiate(lightOverlayPrefab);
        lightMapOverlay.name = lightOverlayName;
        lightMapOverlay.transform.SetParent(newChunk.transform, true);
        lightMapOverlay.transform.localScale = new Vector3(ChunkSize.x, ChunkSize.y, 1);
        lightMapOverlay.transform.position = new Vector2(ChunkCoord * ChunkSize.x + ChunkSize.x / 2f, ChunkSize.y / 2f);
        Texture2D lightMap = new Texture2D(ChunkSize.x, ChunkSize.y);
        Material lightMaterial = new Material(lightShader);
        lightMaterial.SetTexture("_LightMap", lightMap);
        lightMapOverlay.GetComponent<SpriteRenderer>().material = lightMaterial;
        lightMap.filterMode = FilterMode.Point; //< remove this line for smooth lighting, keep it for tiled lighting

        StartCoroutine(ApplyLightToChunk(lightMap, lightDataToApply, pos));
    }

    public IEnumerator DrawChunk(int[,] Data, Vector2Int offset, Action onDrawingComplete = null)
    {
        for (int x = 0; x < ChunkSize.x; x++)
        {
            for (int y = 0; y < ChunkSize.y; y++)
            {
                int currentBlockID = Data[x, y];
                if (currentBlockID != 0)
                {
                    TileObject tileObject = TileObjectRegistry.GetTileObjectByID(Mathf.Abs(currentBlockID)); //Background Wall would be negative ID
                    if (tileObject != null)
                    {
                        PlaceTile(tileObject, x + (offset.x * ChunkSize.x), y, offset, currentBlockID < 0, false);
                    }
                    else
                    {
                        Debug.Log("No tile object found for ID: " + currentBlockID);
                    }
                }

                // Yield return null will wait for the next frame before continuing the loop
                if (y % 70 == 0) // Adjust this value as needed for performance
                {
                    yield return null;
                }
            }
        }

        onDrawingComplete?.Invoke();
    }

    public IEnumerator ApplyLightToChunk(Texture2D chunkTexture, float[,] LightData, Vector2Int offset, Action onDrawingComplete = null)
    {
        for (int x = 0; x < ChunkSize.x; x++)
        {
            for (int y = 0; y < ChunkSize.y; y++)
            {
                chunkTexture.SetPixel(x, y, new Color(0, 0, 0, LightData[x, y]));

                // Yield return null will wait for the next frame before continuing the loop
                if (y % 200 == 0) // Adjust this value as needed for performance
                {
                    yield return null;
                }
            }
        }
        chunkTexture.Apply();
        onDrawingComplete?.Invoke();
    }


    public static void PlaceTile(TileObject tile, int x, int y, Vector2Int chunkID, bool isWall, bool placeByPlayer, bool updateLighting = false) 
    {   //change to data needs to be done somewhere else
        if (y < 0 || y >= ChunkSize.y) return; //we only care about out of scope along y axis
        GameObject tileGameObject = placeByPlayer ? tile.GetPlacedGameObject() :
                                isWall ? tile.GetGeneratedWallGameObjects() :
                                tile.GetGeneratedGameObjects();
        if (!TotalChunks.TryGetValue(chunkID.x, out GameObject game)) Debug.LogError("TotalChunks not found ID: "+chunkID.x);
        tileGameObject.transform.parent = TotalChunks[chunkID.x].transform;
        tileGameObject.transform.position = new Vector2(x + 0.5f, y + 0.5f);
    }


    public void UpdateChunk(int ChunkCoord)
    {
        if (ActiveChunks.ContainsKey(ChunkCoord))
        {
            Vector2Int DataCoords = new Vector2Int(ChunkCoord, 0);
            /*
            GameObject TargetChunk = ActiveChunks[ChunkCoord];
            MeshFilter targetFilter = TargetChunk.GetComponent<MeshFilter>();
            MeshCollider targetCollider = TargetChunk.GetComponent<MeshCollider>();

            StartCoroutine(meshCreator.CreateMeshFromData(WorldData[DataCoords], x =>
            {
                targetFilter.mesh = x;
                targetCollider.sharedMesh = x;
            }));*/
        }
    }

    public static int GetChunkCoordsFromPosition(Vector2 WorldPosition)
    {
        return Mathf.FloorToInt(WorldPosition.x / ChunkSize.x);

    }

    public static Vector2Int WorldToLocalCoords(Vector2Int WorldPosition, int Coords)
    {
        return new Vector2Int
        {
            x = WorldPosition.x - Coords * ChunkSize.x,
            y = WorldPosition.y
        };
    }

    public static int GetDataFromWorldPos(Vector2Int worldPosition)
    {
        Vector2Int chunkCoord = new Vector2Int(GetChunkCoordsFromPosition(worldPosition), 0);
        return WorldData[chunkCoord][(int)(worldPosition.x - chunkCoord.x * ChunkSize.x), worldPosition.y];
    }

    public void AddCoreArchitectureToChunk(int chunkCoord)
    {
        if (TotalChunks.ContainsKey(chunkCoord))
        {
            Vector2Int chunkPosition = new Vector2Int(chunkCoord, 0);

            // Check if WorldData contains the chunk data
            if (!WorldData.ContainsKey(chunkPosition))
            {
                Debug.LogError("World data not found for chunk coordinate: " + chunkCoord);
                return;
            }

            int[,] chunkData = WorldData[chunkPosition];
            int middleX = ChunkSize.x / 2; // Middle x of the chunk
            int highestY = FindHighestBlockInColumn(chunkData, middleX);
            // Calculate the world position for the top of the chunk
            Vector3 topPosition = new Vector3(chunkCoord * ChunkSize.x + middleX, highestY + 1.73f, 0); // +1 to place it above the highest block
            GameObject CAgameObject = coreArchitectureSO.GetSpawnedGameObject();
            CAgameObject.transform.position = topPosition;
        }
        else
        {
            Debug.LogError("Chunk not found for coordinate: " + chunkCoord);
        }
    }

    private int FindHighestBlockInColumn(int[,] chunkData, int x)
    {
        for (int y = ChunkSize.y - 1; y >= 0; y--)
        {
            if (chunkData[x, y] > 0) // Assuming 0 indicates no block
            {
                return y; // Return the y position of the topmost block
            }
        }
        return 0; // Return 0 if no blocks are found in the column
    }

    #region
    public void LoadData(GameData data)
    {
        // Load WorldData and other relevant fields from the GameData object
        if (data.serializableWorldData?.pairs.Count != 0)
        {
            WorldData = data.GetWorldData();

            foreach (var dataToApply in WorldData)
            {
                int chunkCoord = dataToApply.Key.x;
                if (!ActiveChunks.ContainsKey(chunkCoord))
                {
                    StartCoroutine(CreateChunk(chunkCoord,() =>
                    {
                        if (chunkCoord == 0)
                        {
                            AddCoreArchitectureToChunk(chunkCoord);
                        }
                    }));
                }
            }
        }
    }

    public void SaveData(GameData data)
    {
        // Save current world state to the GameData object
        data.serializableWorldData = GameData.SetWorldData(WorldData);
    }
    #endregion
}
