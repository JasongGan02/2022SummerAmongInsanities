using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Experimental.Rendering.RayTracingAccelerationStructure;
using System;

public class DataGenerator
{   

    public class GenData
    {
        public System.Action<TileObject[,,]> OnComplete;
        public Vector2Int GenerationPoint;
    }

    private WorldGenerator GeneratorInstance;
    private Queue<GenData> DataToGenerate;
    private TerrainSettings[] terrainSettings;
    private float seed; 
    float noiseOffsetX; 
    float noiseOffsetY; 
    public bool Terminate;
    System.Random sysRandom;
    //private StructureGenerator structureGen;
    public DataGenerator(WorldGenerator worldGen,  TerrainSettings[] terrainSettings, StructureGenerator structureGen = null)
    {
        GeneratorInstance = worldGen;
        DataToGenerate = new Queue<GenData>();
        //this.structureGen = structureGen;
        this.terrainSettings = terrainSettings;
        seed = GeneratorInstance.seed;
        noiseOffsetX = seed + 12345.6789f; // Use an arbitrary constant to offset the noise
        noiseOffsetY = seed + 98765.4321f; // Different arbitrary constant for Y
        sysRandom = new System.Random(seed.GetHashCode());
        //worldGen.StartCoroutine(DataGenLoop());
    }
    
    public void GenerateAllWorldData(int worldSizeInChunks)
    {
        List<Task> tasks = new List<Task>();
        for (int x = -worldSizeInChunks; x <= worldSizeInChunks; x++)
        {
            Vector2Int chunkCoord = new Vector2Int(x, 0);
            tasks.Add(Task.Run(() =>
            {
                var data = GenerateDataSync(chunkCoord);
                lock (WorldGenerator.WorldData)
                {
                    //Debug.Log(chunkCoord + " is created.");
                    WorldGenerator.WorldData[chunkCoord] = data;
                }
            }));
        }
        Task.WaitAll(tasks.ToArray());
    }
    
    private TileObject[,,] GenerateDataSync(Vector2Int offset)
    {
        Vector2Int ChunkSize = WorldGenerator.ChunkSize;
        TileObject[,,] TempData = new TileObject[ChunkSize.x, ChunkSize.y, WorldGenerator.TileLayers];

        // Biome Determination Logic based on offset
        TerrainSettings curBiomeSettings = GetCurBiome(offset.x);
        OreClass[] ores = curBiomeSettings.ores;

       //InitCaves
        bool[,] cavePoints = new bool[ChunkSize.x, ChunkSize.y];
        for (int x = 0; x < ChunkSize.x; x++)
        {
            float worldX = x + (offset.x * ChunkSize.x);
            float h = GetHeight(worldX, curBiomeSettings);
            for (int y = 0; y < ChunkSize.y; y++)
            {

                float p = y/h;
                float v = Mathf.PerlinNoise((worldX + noiseOffsetX) * curBiomeSettings.caveFrequency, (y + noiseOffsetY) * curBiomeSettings.caveFrequency);
       
                v *= (p + 0.5f);
                float depthThreshold = y > h/2 ? 0 : (h - 0.7f *y) / h * 0.25f;
                cavePoints[x, y] = v > 0.5f - depthThreshold && y <= h;
            }
        }

        //InitOres
        Dictionary<OreClass, bool[,]> spawnMasks = new Dictionary<OreClass, bool[,]>();
        foreach (OreClass ore in ores)
        {
            bool[,] spawnMask = new bool[ChunkSize.x, ChunkSize.y];
            for (int x = 0; x < ChunkSize.x; x++)
            {
                for (int y = 0; y < ChunkSize.y; y++)
                {
                    float worldX = x + (offset.x * ChunkSize.x);
                    float oreFloat = BitConverter.ToSingle(BitConverter.GetBytes(ore.oreTile.itemName.GetHashCode()), 0) % 10;
                    float v = Mathf.PerlinNoise((worldX + noiseOffsetX + oreFloat) * ore.spawnFrequency,
                        (y + noiseOffsetY + oreFloat) * ore.spawnFrequency);
                    spawnMask[x, y] = v <= ore.spawnRadius && y <= ore.maxSpawnHeight && y >= ore.minSpawnHeight;
                }
            }
            spawnMasks[ore] = spawnMask;
        }

        //Data Filling
        for (int x = 0; x < ChunkSize.x; x++)
        {
            float worldX = x + (offset.x * ChunkSize.x);
            float height = GetHeight(worldX, curBiomeSettings);
            for (int y = 0; y < height; y++)
            {
                TileObject tileObject; // grass: 1; dirt: 2; stone: 3
                if (y < height - curBiomeSettings.dirtLayerHeight)
                    tileObject = curBiomeSettings.tileAtlas.stone;
                else if (y < height - 1)
                    tileObject = curBiomeSettings.tileAtlas.dirt;
                else
                    tileObject = curBiomeSettings.tileAtlas.grass; 
                //ores
                foreach (OreClass ore in curBiomeSettings.ores)
                {
                    if (spawnMasks[ore][x, y])
                        tileObject = ore.oreTile;
                }

                //place the tile if we're not in a cave 
                if (cavePoints[x, y])
                    TempData[x, y, tileObject.TileLayer] = tileObject;
                else
                {
                    //Place Wall Tile is represented by tileLayer = 0
                    if (y < height - curBiomeSettings.dirtLayerHeight - sysRandom.Next(2, 5))
                        TempData[x, y, 0] = curBiomeSettings.tileAtlas.stone; 
                    else if (y < height)
                        TempData[x, y, 0] = curBiomeSettings.tileAtlas.dirt;
                }
                //spawn addons
                if (y == height - 1)
                {
                    if (TempData[x, y, 1] == curBiomeSettings.tileAtlas.grass)
                    {
                        if (sysRandom.Next(0, 100) < curBiomeSettings.tallGrassChance)
                            TempData[x, y+1, 3] = curBiomeSettings.tileAtlas.natureAddons; //layer = 3 to ignore lighting
                        else if (sysRandom.Next(0, 100) < curBiomeSettings.treeChance)
                            TempData[x, y + 1, 3] = curBiomeSettings.tileAtlas.tree;
                    }
                }

            }
        }
        
        // After generating the terrain, check if we need to add air blocks at the edges
        int chunkCoord = offset.x;

        if (chunkCoord == -WorldGenerator.worldSizeInChunks)
        {
            // Leftmost chunk; add air blocks on the left edge
            AddAirBlocksToChunkEdges(TempData, chunkCoord, true);
        }
        else if (chunkCoord == WorldGenerator.worldSizeInChunks)
        {
            // Rightmost chunk; add air blocks on the right edge
            AddAirBlocksToChunkEdges(TempData, chunkCoord, false);
        }
        
        return TempData;
    }
    
    private IEnumerator GenerateData(Vector2Int offset, System.Action<TileObject[,,]> callback)
    {
        TileObject[,,] TempData = null;

        Task t = Task.Run(() =>
        {
            TempData = GenerateDataSync(offset);
        });

        yield return new WaitUntil(() => t.IsCompleted || t.IsCanceled);

        if (t.Exception != null)
            Debug.LogError(t.Exception);

        lock (WorldGenerator.WorldData)
        {
            WorldGenerator.WorldData[offset] = TempData;
        }
        callback(TempData);
    }

    private TerrainSettings GetCurBiome(int chunkCoord)
    {
        //TODO: Base on Level to select Biome
        if (chunkCoord >= -2 && chunkCoord <= 2)
        {
            return terrainSettings[1]; // Return base settings for this range
        }
        else
        {
            // Return default terrain settings otherwise
            TerrainSettings curBiomeSettings = terrainSettings[0];
            return curBiomeSettings;
        }
    }

    private int GetHeight(float x, TerrainSettings curBiomeSettings)
    {
        // Apply a non-symmetrical transformation to the seed based on the chunk's position
        // This ensures that the noise function isn't centered around the origin
        float noiseOffsetX = seed + 12345.6789f; // Use an arbitrary constant to offset the noise
        float noiseOffsetY = seed + 98765.4321f; // Different arbitrary constant for Y

        // Shift the x coordinate by a value dependent on the seed and the arbitrary constants
        float noiseX = (x + noiseOffsetX) * curBiomeSettings.terrainFrequency;
        float noiseY = noiseOffsetY * curBiomeSettings.terrainFrequency; // Constant Y value since we're only generating a heightmap

        // Use the shifted values to get noise
        float noiseValue = Mathf.PerlinNoise(noiseX, noiseY);

        // Apply the noise to calculate the height
        return curBiomeSettings.heightAddition + Mathf.CeilToInt(curBiomeSettings.heightMultiplier * noiseValue);
    }

    private void AddAirBlocksToChunkEdges(TileObject[,,] chunkData, int chunkCoord, bool isLeftEdge)
    {
        TerrainSettings curBiome = GetCurBiome(chunkCoord);
        int edgeX = isLeftEdge ? 0 : WorldGenerator.ChunkSize.x - 1;

        for (int y = 0; y < WorldGenerator.ChunkSize.y; y++)
        {
            for (int z = 0; z < WorldGenerator.TileLayers; z++)
            {
                TileObject airBlock = curBiome.tileAtlas.block;
                chunkData[edgeX, y, z] = airBlock;
            }
        }
    }
}
