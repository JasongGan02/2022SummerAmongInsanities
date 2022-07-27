using System.Collections;
using UnityEngine;
using System.Collections.Generic;
public class TerrainGeneration : MonoBehaviour
{
    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;

    [Header("Nature Addons")]
    public int addonsChance;

    [Header("Generation Settings")]
    public int chunkSize = 16;
    public int treeChance = 10;
    public bool generateCave = true;
    public int dirtLayerHeight = 5;
    public int worldSize = 100;
    public float surfacePortion = 0.25f;
    public float heightMultiplier = 15f;
    public int heightAddition = 25;
    
    [Header("Noise Settings")]
    public float terrainFreq = 0.05f;
    public float caveFreq = 0.05f;
    public float seed;
    public Texture2D caveNoiseTexture;

    [Header("Ore Settings")]
    public OreClass[] ores;
    /*public float coalRarity;
    public float coalSize;
    public float ironRarity;
    public float ironSize;
    public float goldRarity, goldSize;
    public Texture2D coalSpread;
    public Texture2D ironSpread;
    public Texture2D goldSpread;*/


    private GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();

    private void OnValidate()
    {

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


    private void Start()
    {
        seed = Random.Range(-10000, 10000);

        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        ores[0].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[1].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[2].spreadTexture = new Texture2D(worldSize, worldSize);



        GenerateNoiseTexture(caveFreq, surfacePortion, caveNoiseTexture);
        //ores
        GenerateNoiseTexture(ores[0].rarity, ores[0].size, ores[0].spreadTexture);
        GenerateNoiseTexture(ores[1].rarity, ores[1].size, ores[1].spreadTexture);
        GenerateNoiseTexture(ores[2].rarity, ores[2].size, ores[2].spreadTexture);


        CreateChunks();
        GenerateTerrain();
        ChangeSize();
        
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
        for (int x = 0; x < worldSize; x++)
        {  
            float height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier +heightAddition;
            
            for (int y = 0; y < height; y++)
            {
                Sprite[] tileSprites;
                if (y < height - dirtLayerHeight)
                {
                    tileSprites = tileAtlas.stone.tileSprites;
                    //ore and stone generation
                    if (ores[0].spreadTexture.GetPixel(x,y).r >0.5f && height - y > ores[0].masSpawnHeight)
                        tileSprites = tileAtlas.coal.tileSprites;
                    if(ores[1].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[1].masSpawnHeight)
                        tileSprites = tileAtlas.iron.tileSprites;
                    if (ores[2].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[2].masSpawnHeight)
                        tileSprites = tileAtlas.gold.tileSprites;
                }
                else if(y < height - 1)
                {
                    tileSprites = tileAtlas.dirt.tileSprites;
                }
                else
                {
                    //top later of the terrain
                    tileSprites = tileAtlas.grass.tileSprites;

                }

                if (generateCave)
                {
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        PlaceTile(tileSprites, x, y, Constants.Layer.GROUND, true, Constants.Tag.GROUND);
                    }
                }
                else
                {
                    PlaceTile(tileSprites, x, y, Constants.Layer.GROUND, true, Constants.Tag.GROUND);
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
                            GenerateTree(x, y);
                        }

                    }
                    else
                    {
                        int i = Random.Range(0, addonsChance);
                        //generate natural stuff like flowers and tall grass
                      
                        if (worldTiles.Contains(new Vector2(x, y)) && i==1)
                        {
                            PlaceTile(tileAtlas.natureAddons.tileSprites, x, y+1);
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
        PlaceTile(tileAtlas.tree.tileSprites, x, y);

    }
    public void PlaceTile(Sprite[] tileSprites, int x, int y, int layer = 0, bool hasCollider = false, string tag = null)
    {
        GameObject newTile = new GameObject();

        float chunkCoord = Mathf.Round(x / chunkSize) * chunkSize;
        chunkCoord /= chunkSize;

        newTile.transform.parent = worldChunks[(int)chunkCoord].transform;
        newTile.AddComponent<SpriteRenderer>();
        int spriteIndex = Random.Range(0, tileSprites.Length);
        newTile.GetComponent<SpriteRenderer>().sprite = tileSprites[spriteIndex];
        newTile.name = tileSprites[0].name;
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);
        newTile.layer = layer;
        if (hasCollider)
        {
            newTile.AddComponent<BoxCollider2D>();
        }
        if (tag != null)
        {
            newTile.tag = tag;
        }

        worldTiles.Add(newTile.transform.position - (Vector3.one*0.5f));
    }
}
