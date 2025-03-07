using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class WorldGenerator : MonoBehaviour, IDataPersistence
{
    public static Dictionary<Vector2Int, TileObject[,,]> WorldData;
    public static Dictionary<Vector2Int, float[,]> WorldLightData;
    public static Dictionary<Vector2Int, Texture2D> WorldLightTexture;
    public static Dictionary<int, GameObject> ActiveChunks;
    public static Dictionary<int, GameObject> TotalChunks;
    public static Dictionary<int, int[,]> AdditiveWorldData;
    public static int worldSizeInChunks = 10;
    public static HashSet<int> ReadyChunks = new HashSet<int>();
    public static readonly Vector2Int ChunkSize = new Vector2Int(32, 100);
    public static int TileLayers = 5; // 0 = walls, 1 = entity blocks like tiles, 2 = accessories, 3 = accessories topmost (3 is ignored calculating light)
    public float seed;
    public TerrainSettings[] settings;
    public static bool CoreGenerated = false; // New flag

    public static WorldGenerator Instance { get; private set; }
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Optional: Keep instance alive across scenes
        }

        WorldData = new Dictionary<Vector2Int, TileObject[,,]>();
        WorldLightData = new Dictionary<Vector2Int, float[,]>();
        WorldLightTexture = new Dictionary<Vector2Int, Texture2D>();
        ActiveChunks = new Dictionary<int, GameObject>();
        TotalChunks = new Dictionary<int, GameObject>();
        RecalculateSeed();
        dataCreator = new DataGenerator(this, settings, GetComponent<StructureGenerator>());
        dataCreator.GenerateAllWorldData(worldSizeInChunks);
        InitializeWorld();
    }

    private void RecalculateSeed()
    {
        if (seed == 0) seed = UnityEngine.Random.Range(-10000, 10000);
    }

    public void InitializeWorld()
    {
        StartCoroutine(InitializeWorldCoroutine());
    }

    private IEnumerator InitializeWorldCoroutine()
    {
        for (int x = -worldSizeInChunks; x <= worldSizeInChunks; x++)
        {
            int chunkCoord = x;
            // Start creating each chunk
            yield return StartCoroutine(CreateChunk(chunkCoord, () =>
            {
                if (chunkCoord == 0)
                {
                    AddCoreArchitectureToChunk(chunkCoord);
                }
            }));
        }
    }

    public IEnumerator CreateChunk(int ChunkCoord, Action onChunkCreated = null)
    {
        Vector2Int pos = new Vector2Int(ChunkCoord, 0);
        if (ActiveChunks.ContainsKey(ChunkCoord))
            yield break;

        if (TotalChunks.ContainsKey(ChunkCoord))
        {
            TotalChunks[ChunkCoord].SetActive(true);
            ActiveChunks.Add(ChunkCoord, TotalChunks[ChunkCoord]);
            RefreshChunkLight(pos, true);
            yield break;
        }

        string chunkName = $"Chunk {ChunkCoord}";
        GameObject newChunk = new GameObject(chunkName);
        newChunk.transform.position = new Vector2(ChunkCoord * ChunkSize.x, 0f);
        newChunk.transform.SetParent(transform, true);
        ActiveChunks.Add(ChunkCoord, newChunk);
        SetUpNewChunkWithContainers(newChunk);

        // Wait until world data for this chunk is ready.
        yield return new WaitUntil(() => WorldData.ContainsKey(pos));
        TileObject[,,] dataToApply = WorldData[pos];

        if (!TotalChunks.ContainsKey(ChunkCoord))
            TotalChunks.Add(ChunkCoord, newChunk);

        // Create a safe callback that only invokes onChunkCreated once.
        bool callbackCalled = false;
        Action safeCallback = () =>
        {
            if (!callbackCalled)
            {
                onChunkCreated?.Invoke();
                callbackCalled = true;
            }
        };

        // Draw the chunk and invoke the safe callback when drawing is complete.
        bool isChunkDrawn = false;
        StartCoroutine(DrawChunk(dataToApply, pos, () =>
        {
            safeCallback();
            isChunkDrawn = true;
        }));
        yield return new WaitUntil(() => isChunkDrawn);

        // Process light.
        float[,] lightDataToApply = WorldLightData.ContainsKey(pos) ? WorldLightData[pos] : null;
        if (lightDataToApply == null)
        {
            LightGenerator.Instance.QueueDataToGenerate(new LightGenerator.GenData
            {
                GenerationPoint = pos,
                OnComplete = x => { lightDataToApply = x; },
                UpdateNeighbors = true
            });
            yield return new WaitUntil(() => lightDataToApply != null);
        }

        string lightOverlayName = $"Light Overlay {ChunkCoord}";
        GameObject lightMapOverlay = Instantiate(lightOverlayPrefab);
        lightMapOverlay.name = lightOverlayName;

        Texture2D lightMap = new Texture2D(ChunkSize.x, ChunkSize.y);
        Material lightMaterial = new Material(lightShader);
        lightMaterial.SetTexture("_LightMap", lightMap);
        lightMapOverlay.GetComponent<SpriteRenderer>().material = lightMaterial;
        lightMap.filterMode = FilterMode.Point;
        if (!WorldLightTexture.ContainsKey(pos))
            WorldLightTexture.Add(pos, lightMap);

        // Apply light and then invoke the safe callback once more.
        StartCoroutine(ApplyLightToChunkCoroutine(lightMap, lightDataToApply, () =>
        {
            safeCallback();
            lightMapOverlay.transform.SetParent(newChunk.transform, true);
            lightMapOverlay.transform.localScale = new Vector3(ChunkSize.x, ChunkSize.y, 1);
            lightMapOverlay.transform.position = new Vector2(ChunkCoord * ChunkSize.x + ChunkSize.x / 2f, ChunkSize.y / 2f);
            lightMapOverlay.SetActive(true);
            ReadyChunks.Add(ChunkCoord);
        }));
    }

    private static void SetUpNewChunkWithContainers(GameObject newChunk)
    {
        GameObject tiles = new GameObject("Tiles");
        tiles.transform.SetParent(newChunk.transform, true);
        GameObject chunkMobContainer = new GameObject("MobContainer");
        chunkMobContainer.transform.SetParent(newChunk.transform, true);
        GameObject enemyContainer = new GameObject("EnemyContainer");
        enemyContainer.transform.SetParent(chunkMobContainer.transform, true);
        GameObject towerContainer = new GameObject("TowerContainer");
        towerContainer.transform.SetParent(newChunk.transform, true);
        GameObject dropContainer = new GameObject("DropContainer");
        dropContainer.transform.SetParent(newChunk.transform, true);
    }

    public IEnumerator DrawChunk(TileObject[,,] Data, Vector2Int offset, Action onDrawingComplete = null)
    {
        for (int x = 0; x < ChunkSize.x; x++)
        {
            for (int y = 0; y < ChunkSize.y; y++)
            {
                for (int z = 0; z < Data.GetLength(2); z++)
                {
                    TileObject tileObject = Data[x, y, z];
                    if (tileObject != null)
                    {
                        PlaceTile(tileObject, x + (offset.x * ChunkSize.x), y, offset, z == 0, false);
                    }
                }
            }
        }

        onDrawingComplete?.Invoke();

        yield return null;
    }

    public void RefreshChunkLight(Vector2Int offset, bool updateNeighbors = false, Action onRefreshComplete = null)
    {
        StartCoroutine(RefreshChunkLightCoroutine(offset, updateNeighbors, () => onRefreshComplete?.Invoke()));
    }

    public IEnumerator RefreshChunkLightCoroutine(Vector2Int offset, bool updateNeighbors = false, Action onRefreshComplete = null)
    {
        yield return new WaitForEndOfFrame();
        float[,] lightDataToApply = null;

        LightGenerator.Instance.QueueDataToGenerate(new LightGenerator.GenData
        {
            GenerationPoint = offset,
            OnComplete = x => lightDataToApply = x,
            UpdateNeighbors = updateNeighbors
        });

        yield return new WaitUntil(() => lightDataToApply != null);
        if (WorldLightTexture.ContainsKey(offset))
            StartCoroutine(ApplyLightToChunkCoroutine(WorldLightTexture[offset], lightDataToApply, () => onRefreshComplete?.Invoke()));
    }

    public void ApplyLightToChunk(Texture2D chunkTexture, float[,] LightData, Action onDrawingComplete = null)
    {
        StartCoroutine(ApplyLightToChunkCoroutine(chunkTexture, LightData, () => onDrawingComplete?.Invoke()));
    }

    private IEnumerator ApplyLightToChunkCoroutine(Texture2D chunkTexture, float[,] LightData, Action onDrawingComplete = null)
    {
        yield return new WaitForEndOfFrame();
        for (int x = 0; x < ChunkSize.x; x++)
        {
            for (int y = 0; y < ChunkSize.y; y++)
            {
                chunkTexture.SetPixel(x, y, new Color(0, 0, 0, LightData[x, y]));
            }
        }

        chunkTexture.Apply();
        onDrawingComplete?.Invoke();
    }


    public static void PlaceTile(TileObject tile, int x, int y, Vector2Int chunkID, bool isWall, bool placeByPlayer, bool updateLighting = false)
    {
        if (y < 0 || y >= ChunkSize.y) return;

        int spriteNumber = 0;
        Quaternion rotation = Quaternion.identity;
        bool flipX = false;

        // 检查方块是否有特殊功能
        if (tile.HasSpecialFunctionality && tile.SpecifiedTiles.Length > 0)
        {
            Vector2Int worldPosition = new Vector2Int(x, y);

            // 如果是草地方块，获取草地的特定 sprite 和旋转信息
            if (tile.IsGrassTile)
            {
                (spriteNumber, rotation, flipX) = TileHelper.GetGrassTileSpriteAndRotation(worldPosition, tile.SpecifiedTiles);
            }
            // 否则，检查其他特殊方块
            else if (tile.AnotherSpecifiedTiles.Length > 0)
            {
                // 尝试获取主要的 sprite 和旋转
                (spriteNumber, rotation) = TileHelper.GetSpriteNumberAndRotation(worldPosition, tile.SpecifiedTiles);

                // 如果 spriteNumber 为 0，使用备用的 sprite 获取方法
                if (spriteNumber == 0)
                {
                    (spriteNumber, rotation) = TileHelper.GetAnotherSpriteNumberAndRotation(worldPosition, tile.TileLayer, tile.AnotherSpecifiedTiles);
                }
            }
            else
            {
                // 没有其他特殊情况时，使用默认的 sprite 获取逻辑
                (spriteNumber, rotation) = TileHelper.GetSpriteNumberAndRotation(worldPosition, tile.SpecifiedTiles);
            }
        }

        GameObject tileGameObject = placeByPlayer
            ? tile.GetPlacedGameObject(spriteNumber, rotation, flipX)
            : isWall
                ? tile.GetGeneratedWallGameObjects(spriteNumber, rotation, flipX)
                : tile.GetGeneratedGameObjects(spriteNumber, rotation, flipX);

        if (!TotalChunks.TryGetValue(chunkID.x, out GameObject chunk))
        {
            Debug.LogError("TotalChunks not found ID: " + chunkID.x);
            return;
        }

        tileGameObject.transform.SetParent(chunk.transform.Find("Tiles").transform, true);
        tileGameObject.transform.position = new Vector2(x + 0.5f, y + 0.5f);
        var dynamicLightController = tileGameObject.GetComponent<DynamicLightController>();
        if (dynamicLightController != null)
        {
            dynamicLightController.Initialize(tile, new Vector2(x + 0.5f, y + 0.5f));
        }

        if (updateLighting)
            Instance.RefreshChunkLight(chunkID, true);
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

    public static TileObject GetDataFromWorldPos(Vector2Int worldPosition)
    {
        Vector2Int chunkCoord = new Vector2Int(GetChunkCoordsFromPosition(worldPosition), 0);
        return WorldData[chunkCoord][(int)(worldPosition.x - chunkCoord.x * ChunkSize.x), worldPosition.y, 1];
    }

    public static bool IsCurTileEmpty(Vector2Int worldPosition)
    {
        Vector2Int chunkCoord = new Vector2Int(GetChunkCoordsFromPosition(worldPosition), 0);

        int layersCount = WorldData[chunkCoord].GetLength(2); // Get the number of layers.

        for (int layer = 1; layer < layersCount; layer++) // layer = 0 ʱΪǽ�壬����ʵ�巽��
        {
            // Check each layer from 0 upwards for the given worldPosition
            if (WorldData[chunkCoord][(int)(worldPosition.x - chunkCoord.x * ChunkSize.x), worldPosition.y, layer] != null)
            {
                // If any layer is not null (i.e., contains something), return false indicating the tile is not empty
                return false;
            }
        }

        // If all layers from 0 upwards are null (empty), return true
        return true;
    }


    public void AddCoreArchitectureToChunk(int chunkCoord)
    {
        if (!TotalChunks.ContainsKey(chunkCoord))
        {
            Debug.LogError("Chunk not found for coordinate: " + chunkCoord);
            return;
        }

        Vector2Int chunkPosition = new Vector2Int(chunkCoord, 0);
        if (!WorldData.ContainsKey(chunkPosition))
        {
            Debug.LogError("World data not found for chunk coordinate: " + chunkCoord);
            return;
        }

        TileObject[,,] chunkData = WorldData[chunkPosition];
        int middleX = ChunkSize.x / 2; // Middle x of the chunk

        // Find the highest non-null block in the middle column, replace it with grass, and place the core above it
        int highestY = FindHighestBlockInColumn(chunkData, middleX);

        if (highestY < 0)
        {
            Debug.LogError("No valid ground found in chunk: " + chunkCoord);
            return;
        }

        // Place the core architecture object
        Vector3 corePosition = new Vector3(chunkCoord * ChunkSize.x + middleX + 0.5f, highestY + 1f, 0); // Adjust the Y position as needed
        GameObject coreGameObject = coreArchitectureSO.GetSpawnedGameObject();
        coreGameObject.transform.position = corePosition;

        CoreGenerated = true;
    }

    private int FindHighestBlockInColumn(TileObject[,,] chunkData, int x)
    {
        for (int y = ChunkSize.y - 1; y >= 0; y--)
        {
            if (chunkData[x, y, 1] != null) // Assuming layer 1 is the entity layer
            {
                return y;
            }
        }

        return -1; // Indicate no block found
    }

    #region

    public void LoadData(GameData data)
    {
        // Load WorldData and other relevant fields from the GameData object
        if (data.serializableWorldData?.pairs.Count != 0)
        {
            //WorldData = data.GetWorldData();

            foreach (var dataToApply in WorldData)
            {
                int chunkCoord = dataToApply.Key.x;
                if (!ActiveChunks.ContainsKey(chunkCoord))
                {
                    StartCoroutine(CreateChunk(chunkCoord, () =>
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
        //data.serializableWorldData = GameData.SetWorldData(WorldData);
    }

    #endregion
}