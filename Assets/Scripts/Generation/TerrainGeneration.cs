using System.Collections;
using UnityEngine;
using System.Collections.Generic;
public class TerrainGeneration : MonoBehaviour
{
    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;
    public float seed;

    public BiomeClass[] biomes;

    public BiomeClass MainArea;
    public BiomeClass Kingdom1;
    public BiomeClass Kingdom2;
    public BiomeClass NetherWorld;
    public BiomeClass ForbiddenKingdom;


    [Header("Biomes")]
    public float biomeFrequency;
    public Gradient biomeColors;    
    public Texture2D biomeMap;

    [Header("Nature Addons")]
    public int addonsChance;
    public int treeChance = 10;

    [Header("Generation Settings")]
    public int worldSize = 100;
    public int chunkSize = 16;
    public bool generateCave = true;
    public int dirtLayerHeight = 5;
    public float surfacePortion = 0.25f;
    public float heightMultiplier = 15f;
    public int heightAddition = 25;
    
    [Header("Noise Settings")]
    public float terrainFreq = 0.05f;
    public float caveFreq = 0.05f;
    public Texture2D caveNoiseTexture;

    [Header("Ore Settings")]
    public OreClass[] ores;


    private GameObject[] worldChunks;
    private HashSet<Vector2> worldTiles = new HashSet<Vector2>();

    private void OnValidate()
    {
        DrawTexture();
        
    }


    private void Start()
    {
        seed = Random.Range(-10000, 10000);
        DrawTexture();
        CreateChunks();
        GenerateTerrain();
        ChangeSize();
    }

    public void DrawTexture()
    {
        biomeMap = new Texture2D(worldSize,worldSize);
        DrawBiomeTexture();
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        ores[0].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[1].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[2].spreadTexture = new Texture2D(worldSize, worldSize);

        GenerateNoiseTexture(caveFreq, surfacePortion, caveNoiseTexture);
        //ores
        GenerateNoiseTexture(ores[0].rarity, ores[0].size, ores[0].spreadTexture);
        GenerateNoiseTexture(ores[1].rarity, ores[1].size, ores[1].spreadTexture);
        GenerateNoiseTexture(ores[2].rarity, ores[2].size, ores[2].spreadTexture);
    }

    public void DrawBiomeTexture()
    {
        for(int x = 0; x<biomeMap.width; x++)
        {
            for (int y=0; y<biomeMap.height; y++)
            {
                float v = Mathf.PerlinNoise((x+seed)* biomeFrequency, (y+seed)* biomeFrequency);
                Color col = biomeColors.Evaluate(v);
                biomeMap.SetPixel(x,y, col);
            }
        }
        biomeMap.Apply();
    }
    public void ChangeSize()
    {
        GameObject.Find("TerrainGenerator").transform.localScale = new Vector2(0.25f, 0.25f);
    }

    public void CreateChunks()
    {
        int numChunks = worldSize / chunkSize;
        worldChunks = new GameObject[numChunks];
        for (int i = 0; i<numChunks; i++)
        {
            GameObject newChunk = new GameObject();
            newChunk.name = i.ToString();
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;
        }
    }
    
    public void GenerateTerrain()
    {
        float mid = Mathf.Round(worldSize/2);
        for (int x = 0; x < worldSize; x++)
        {  
            float height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier +heightAddition;
            
            for (int y = 0; y < height; y++)
            {
                TileClass tileSprites;
                if (y < height - dirtLayerHeight)
                {
                    tileSprites = tileAtlas.stone;
                    //ore and stone generation
                    if (ores[0].spreadTexture.GetPixel(x,y).r >0.5f && height - y > ores[0].masSpawnHeight)
                        tileSprites = tileAtlas.coal;
                    if(ores[1].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[1].masSpawnHeight)
                        tileSprites = tileAtlas.iron;
                    if (ores[2].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[2].masSpawnHeight)
                        tileSprites = tileAtlas.gold;
                }
                else if(y < height - 1)
                {
                    tileSprites = tileAtlas.dirt;
                }
                else
                {
                    //top later of the terrain
                    tileSprites = tileAtlas.grass;

                }

                if (generateCave)
                {
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        PlaceTile(tileSprites, x, y);
                    }
                }
                else
                {
                    PlaceTile(tileSprites, x, y);
                }

                //tree
                if (y >= height-1)
                {
                    int t = Random.Range(0, treeChance);
                    if (t == 1)
                    {
                        //generate a tree
                        if (worldTiles.Contains(new Vector2(x, y)))
                        {
                            GenerateTree(x, y+1);
                        }

                    }
                    else
                    {
                        int i = Random.Range(0, addonsChance);
                        //generate natural stuff like flowers and tall grass
                      
                        if (worldTiles.Contains(new Vector2(x, y)) && i==1)
                        {
                            PlaceTile(tileAtlas.natureAddons, x, y+1);
                        }
                    }
                }
            }
        }
    }
    public void GenerateNoiseTexture(float frequency, float limit, Texture2D noiseTexture)
    {
            
        for(int x = 0; x<noiseTexture.width; x++)
        {
            for (int y=0; y<noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x+seed)* frequency, (y+seed)* frequency);
                if (v > limit)
                {
                    noiseTexture.SetPixel(x, y, Color.white);
                }
                else
                {
                    noiseTexture.SetPixel(x, y, Color.black);
                }
                    
            }
        }
        noiseTexture.Apply();
    }
    
    void GenerateTree(int x, int y)
    {
        PlaceTile(tileAtlas.tree, x, y);
    }

    public void PlaceTile(TileClass tile, int x, int y)
    {
        var prefabs = tile.prefabs;

            float chunkCoord = Mathf.Round(x / chunkSize) * chunkSize;
            chunkCoord /= chunkSize;

        GameObject prefab = prefabs[(int)Random.Range(0, prefabs.Length)];
        GameObject gameObject = Instantiate(prefab);
        gameObject.transform.parent = worldChunks[(int)chunkCoord].transform;
        gameObject.name = tile.tileName;
        gameObject.transform.position = new Vector2(x + 0.5f, y + 0.5f);
        worldTiles.Add(gameObject.transform.position - (Vector3.one * 0.5f));
    }
}