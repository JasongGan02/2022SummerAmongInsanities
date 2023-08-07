using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

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
    public float height;
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

    // TODO can be replaced by the below dictionary.
    private HashSet<Vector2> worldTiles = new HashSet<Vector2>();
    
    [HideInInspector] public static Dictionary<Vector2Int, GameObject> worldTilesDictionary = new();
    private ShadowGenerator shadowGenerator;
    public Dictionary<Vector2Int, GameObject> currentTerrain;
    public static int groundLayer;
    private static Vector2 PlayerPosition;
    static GameObject Player;
    [HideInInspector] public static Dictionary<Vector2, GameObject> TileWithShadowDictionary= new();
    static List<Vector2> coordinatesToRemove;

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

        shadowGenerator = FindObjectOfType<ShadowGenerator>();
        if (shadowGenerator != null)
        {
            shadowGenerator.Initialize(worldTilesDictionary, worldSize);
        }

        groundLayer = LayerMask.GetMask("ground");
    }
    public void Update()
    {
        //PlayerUpdate();
        //ShadowUpdate();
        //ShadowClose();
    }

    public static void PlayerUpdate()
    {
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }
        else
        {
            PlayerPosition = Player.transform.position;
        }
    }

    public static void ShadowUpdate()
    {
        int count = 0;

        for (float x = PlayerPosition.x - 13f ; x < PlayerPosition.x + 13f; x += 0.25f)
        {
            for (float y = PlayerPosition.y + 7f; y > PlayerPosition.y - 6.5f; y -= 0.25f) 
            {
                Vector2 currentCoordinate = new Vector2(x, y);
                RaycastHit2D hit = Physics2D.Raycast(currentCoordinate, Vector2.down, 0f, groundLayer);

                if (hit.collider != null)
                {
                    GameObject currentTile = hit.collider.gameObject;
                    ShadowCaster2D shadowCaster = currentTile.GetComponent<ShadowCaster2D>();

                    if (shadowCaster != null)
                    {
                        if (shadowCaster.enabled == false)
                        {
                            shadowCaster.enabled = true;
                            if (TileWithShadowDictionary.ContainsKey(currentCoordinate) == false)
                            {
                                TileWithShadowDictionary.Add(currentCoordinate, currentTile);
                            }
                        }

                        count++;
                        if (count > 1)
                        {
                            count = 0;
                            break;
                        }
                    }
                }

            }
        }
    }
    
    public static void ShadowClose()
    {
        coordinatesToRemove = new List<Vector2>();
        
        foreach (KeyValuePair<Vector2, GameObject> pair in TileWithShadowDictionary)
        {
            Vector2 coordinate = pair.Key;
            GameObject tileObject = pair.Value;

            if (coordinate.x > PlayerPosition.x + 13f || coordinate.x < PlayerPosition.x - 13f && tileObject != null)
            {
                if (tileObject.GetComponent<ShadowCaster2D>() != null) 
                { 
                    tileObject.GetComponent<ShadowCaster2D>().enabled = false;
                    coordinatesToRemove.Add(coordinate);
                }
            } 
            else if (coordinate.y < PlayerPosition.y - 6.5f || coordinate.y > PlayerPosition.y + 7.5f && tileObject != null)
            {
                if (tileObject.GetComponent<ShadowCaster2D>() != null) 
                {
                    tileObject.GetComponent<ShadowCaster2D>().enabled = false;
                    coordinatesToRemove.Add(coordinate);
                }
            }
        }
        foreach (Vector2 coordinate in coordinatesToRemove)
        {
            TileWithShadowDictionary.Remove(coordinate);
        }
        //Debug.Log(TileWithShadowDictionary.Count);
    }

    private void RemoveLightSource(int x, int y)
    {
        /*unlitBlocks.Clear();
        UnlightBlock(x, y, x, y);

        List<Vector2Int> toRelight = new();
        foreach (Vector2Int block in unlitBlocks)
        {
            for (int nx = x - 1; nx < x + 2; nx++)
            {
                for (int ny = y - 1; ny < y + 2; ny++)
                {
                    if (lightMap.GetPixel(nx, ny) != null)
                    {
                        if (lightMap.GetPixel(nx, ny).r > lightMap.GetPixel(block.x, block.y).r)
                        {
                            if (!toRelight.Contains(new Vector2Int(nx, ny)))
                            {
                                toRelight.Add(new Vector2Int(nx, ny));
                            }
                        }
                    }
                }
            }
        }

        foreach (Vector2Int source in toRelight)
        {
            LightBlock(source.x, source.y, lightMap.GetPixel(source.x, source.y).r, 0);
        }

        lightMap.Apply();*/
    }

    private void UnlightBlock(int x, int y, int initialX, int initialY)
    {
        /*if (Mathf.Abs(x - initialX) >= lightRadius || Mathf.Abs(y - initialY) >= lightRadius || unlitBlocks.Contains(new Vector2Int(x, y)))
        {
            return;
        }

        for (int nx = x - 1; nx < x + 2; nx++)
        {
            for (int ny = y - 1; ny < y + 2; ny++)
            {
                if (!(nx == x && ny == y))
                {
                    Color targetTile = lightMap.GetPixel(nx, ny);
                    if (targetTile != null && targetTile.r < lightMap.GetPixel(x, y).r)
                    {
                        UnlightBlock(nx, ny, initialX, initialY);
                    }
                }
            }
        }

        lightMap.SetPixel(x, y, Color.black);
        unlitBlocks.Add(new Vector2Int(x, y));*/
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
                Color col = biomeColors?.Evaluate(v) ?? Color.white;
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
        float mid = Mathf.Round(worldSize / 2);
        for (int x = 0; x < worldSize; x++)
        {  
            height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier +heightAddition;
            
            for (int y = 0; y < height; y++)
            {
                IGenerationObject tileSprites;
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
                        GameObject tile = PlaceTile(tileSprites, x, y);
                        worldTilesDictionary.Add(new Vector2Int(x, y), tile);
                    }
                }
                else
                {
                    GameObject tile = PlaceTile(tileSprites, x, y);
                    worldTilesDictionary.Add(new Vector2Int(x, y), tile);
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
        currentTerrain = worldTilesDictionary;
        
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

    private GameObject PlaceTile(IGenerationObject tile, int x, int y)
    {
        var tileGameObject = tile.GetGeneratedGameObjects();

        float chunkCoord = Mathf.Round(x / chunkSize) * chunkSize;
        chunkCoord /= chunkSize;

        tileGameObject.transform.parent = worldChunks[(int)chunkCoord].transform;
        tileGameObject.transform.position = new Vector2(x + 0.5f, y + 0.5f);
        //worldTilesDictionary.Add(new Vector2Int(x, y), tileGameObject);
        worldTiles.Add(tileGameObject.transform.position - (Vector3.one * 0.5f));

        return tileGameObject;
    }
}
