using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;

public class DataGenerator
{   

    public class GenData
    {
        public System.Action<int[,]> OnComplete;
        public Vector2Int GenerationPoint;
    }

    private WorldGenerator GeneratorInstance;
    private Queue<GenData> DataToGenerate;
    private TerrainSettings[] terrainSettings;
    private float seed;
    float noiseOffsetX; 
    float noiseOffsetY; 
    public bool Terminate;

    private StructureGenerator structureGen;
    public DataGenerator(WorldGenerator worldGen,  TerrainSettings[] terrainSettings, StructureGenerator structureGen = null)
    {
        GeneratorInstance = worldGen;
        DataToGenerate = new Queue<GenData>();
        this.structureGen = structureGen;
        this.terrainSettings = terrainSettings;
        seed = WorldGenerator.seed;
        noiseOffsetX = seed + 12345.6789f; // Use an arbitrary constant to offset the noise
        noiseOffsetY = seed + 98765.4321f; // Different arbitrary constant for Y
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
                yield return GeneratorInstance.StartCoroutine(GenerateData(gen.GenerationPoint, gen.OnComplete));
            }

            yield return null;
        }
    }

    public IEnumerator GenerateData(Vector2Int offset, System.Action<int[,]> callback)
    {
        Vector2Int ChunkSize = WorldGenerator.ChunkSize;
        int[,] TempData = new int[ChunkSize.x, ChunkSize.y];
        /*
        if (WorldGenerator.AdditiveWorldData.TryGetValue(offset.x, out int[,] addedData))
        { // new
            TempData = addedData;
            WorldGenerator.AdditiveWorldData.Remove(offset.x);
        }*/
        System.Random sysRandom = new System.Random(seed.GetHashCode());

        //Biome Determination Logic base on offset
        TerrainSettings curBiomeSettings = GetCurBiome(offset.x);
        OreClass[] ores = curBiomeSettings.ores;
        Task t = Task.Factory.StartNew(delegate
        {
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
                        float v = Mathf.PerlinNoise((worldX + noiseOffsetX + ore.oreTile.TileID) * ore.spawnFrequency,
                            (y + noiseOffsetY + ore.oreTile.TileID) * ore.spawnFrequency);
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
                    int tileToPlace; // grass: 1; dirt: 2; stone: 3
                    if (y < height - curBiomeSettings.dirtLayerHeight)
                        tileToPlace = curBiomeSettings.tileAtlas.stone.TileID;
                    else if (y < height - 1)
                        tileToPlace = curBiomeSettings.tileAtlas.dirt.TileID; 
                    else
                        tileToPlace = curBiomeSettings.tileAtlas.grass.TileID; 
                    //ores
                    foreach (OreClass ore in curBiomeSettings.ores)
                    {
                        if (spawnMasks[ore][x, y])
                            tileToPlace = ore.oreTile.TileID;
                    }

                    //place the tile if we're not in a cave 
                    if (cavePoints[x, y])
                        TempData[x, y] = tileToPlace;
                    else
                    {
                        //Place Wall Tile is represented by negative ID
                        if (y < height - curBiomeSettings.dirtLayerHeight - sysRandom.Next(2, 5))
                            TempData[x, y] = - curBiomeSettings.tileAtlas.stone.TileID; 
                        else if (y < height)
                            TempData[x, y] = - curBiomeSettings.tileAtlas.dirt.TileID;
                    }
                    //spawn addons
                    if (y == height - 1)
                    {
                        if (TempData[x, y] == curBiomeSettings.tileAtlas.grass.TileID)
                        {
                            if (sysRandom.Next(0, 100) < curBiomeSettings.tallGrassChance)
                                TempData[x, y+1] = curBiomeSettings.tileAtlas.natureAddons.TileID;
                            else if (sysRandom.Next(0, 100) < curBiomeSettings.treeChance)
                                TempData[x, y+1] = curBiomeSettings.tileAtlas.tree.TileID;
                        }
                    }

                }
            }
            
        });

        yield return new WaitUntil(() => {
            return t.IsCompleted || t.IsCanceled;
        });

        if (t.Exception != null)
            Debug.LogError(t.Exception);

        WorldGenerator.WorldData.Add(offset, TempData);
        callback(TempData);
    }

    private TerrainSettings GetCurBiome(int chunkCoord)
    {
        TerrainSettings curBiomeSettings = terrainSettings[0];
        return curBiomeSettings;
    }

    public int GetHeight(float x, TerrainSettings curBiomeSettings)
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

}
