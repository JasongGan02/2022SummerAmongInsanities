using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class TerrainGeneration : MonoBehaviour
{
    public float seed;
    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;

    public BiomeClass[] biomes;

    [Header("Biomes")]
    public float biomeFrequency;
    public Gradient biomeGradient;
    public Texture2D biomeMap;

    [Header("Generation Settings")]
    public int terrainSize = 100;
    public float height;
    public int chunkSize = 16;
    public int heightAddition = 25;
    public bool generateCave = true;

    [Header("Base Area Settings")]
    public GameObject coreArchPrefab;
    public int flatAreaStartX = 50; // Starting X-coordinate of the flat area
    public int flatAreaEndX = 150; // Ending X-coordinate of the flat area
    public float flatHeightStart = 60; // Y-coordinate height of the flat area
    public float flatHeightEnd = 40; // Ending Y-coordinate height of the no-cave zone
    public int transitionWidth = 5; // Width of the transition area

    [Header("Noise Settings")]
    public float terrainFreq = 0.05f;
    public float caveFreq = 0.05f;
    public Texture2D caveNoiseTexture;

    [Header("Ore Settings")]
    public OreClass[] ores;

    private GameObject[] worldChunks;
    [HideInInspector] public static Dictionary<Vector2Int, GameObject> worldTilesDictionary = new();
    private BiomeClass curBiome;
    public Color[] biomeCols;

    public float occlussion = 7f;
    //For Shadow

    #region

    private ShadowGenerator shadowGenerator;
    public Dictionary<Vector2Int, GameObject> currentTerrain;
    public static int groundLayer;
    private static Vector2 PlayerPosition;
    static GameObject Player;
    [HideInInspector] public static Dictionary<Vector2, GameObject> TileWithShadowDictionary = new();
    static List<Vector2> coordinatesToRemove;

    #endregion

    private void Start()
    {
        seed = Random.Range(-10000, 10000);

        for (int i = 0; i < ores.Length; i++)
        {
            //ores[i].spreadTexture = new Texture2D(terrainSize, terrainSize);
        }

        biomeCols = new Color[biomes.Length];
        for (int i = 0; i < biomes.Length; i++)
        {
            biomeCols[i] = biomes[i].biomeColor;
        }

        DrawBiomeMap();
        DrawCavesAndOres();
        CreateChunks();
        GenerateTerrain();
        //AddCore();

        shadowGenerator = FindObjectOfType<ShadowGenerator>();
        if (shadowGenerator != null)
        {
            shadowGenerator.Initialize(worldTilesDictionary, terrainSize, curBiome.dirtLayerHeight);
            shadowGenerator.IUpdate();
        }

        groundLayer = LayerMask.GetMask("ground");
    }

    #region

    public void Update()
    {
        PlayerUpdate();
        RefreshChunks();
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

    #endregion

    void RefreshChunks()
    {
        for (int i = 0; i < worldChunks.Length; i++)
        {
            if (Vector2.Distance(new Vector2((i * chunkSize) + (chunkSize / 2), 0), new Vector2(PlayerPosition.x,
                    0)) > 8 * occlussion)
            {
                worldChunks[i].SetActive(false);
            }
            else
            {
                worldChunks[i].SetActive(true);
            }
        }
    }

    public void DrawBiomeMap()
    {
        float b;
        Color col;
        biomeMap = new Texture2D(terrainSize, terrainSize);
        for (int x = 0; x < biomeMap.width; x++)
        {
            for (int y = 0; y < biomeMap.height; y++)
            {
                b = Mathf.PerlinNoise((x + seed) * biomeFrequency, (y + seed) * biomeFrequency);
                col = biomeGradient?.Evaluate(b) ?? Color.white;
                biomeMap.SetPixel(x, y, col);
            }
        }

        biomeMap.Apply();
    }

    public void DrawCavesAndOres()
    {
        caveNoiseTexture = new Texture2D(terrainSize, terrainSize);
        float v, o;
        for (int x = 0; x < terrainSize; x++)
        {
            for (int y = 0; y < terrainSize; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                v = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);
                if (x >= flatAreaStartX && x <= flatAreaEndX && y >= flatHeightStart && y <= flatHeightEnd) // Prevent caves in a certain height range within the flat area
                {
                    caveNoiseTexture.SetPixel(x, y, Color.black);
                }
                else if (v > curBiome.surfacePortion)
                {
                    caveNoiseTexture.SetPixel(x, y, Color.white);
                }
                else
                {
                    caveNoiseTexture.SetPixel(x, y, Color.black);
                }

                for (int i = 0; i < ores.Length; i++)
                {
                    /*
                    ores[i].spreadTexture.SetPixel(x, y, Color.black);
                    if (curBiome.ores.Length >= i + 1)
                    {
                        o = Mathf.PerlinNoise((x + seed) * curBiome.ores[i].rarity, (y + seed) * curBiome.ores[i].rarity);
                        if (o > curBiome.ores[i].size)
                        {
                            ores[i].spreadTexture.SetPixel(x, y, Color.white);
                        }
                        ores[i].spreadTexture.Apply();
                    }*/
                }
            }
        }

        caveNoiseTexture.Apply();
    }

    public void DrawTexture()
    {
        /*
        biomeMap = new Texture2D(terrainSize, terrainSize);
        for (int i = 0; i < biomes.Length; i++)
        {
            biomes[i].caveNoiseTexture = new Texture2D(terrainSize, terrainSize);
            for (int o = 0; o < biomes[i].ores.Length; o++)
            {
                biomes[i].ores[o].spreadTexture = new Texture2D(terrainSize, terrainSize);
                GenerateNoiseTextures(biomes[i].ores[o].rarity, biomes[i].ores[o].size, biomes[i].ores[o].spreadTexture);
            }
        }*/
    }

    public void CreateChunks()
    {
        int numChunks = terrainSize / chunkSize;
        worldChunks = new GameObject[numChunks];
        for (int i = 0; i < numChunks; i++)
        {
            GameObject newChunk = new GameObject();
            newChunk.name = i.ToString();
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;
        }
    }

    public BiomeClass GetCurrentBiome(int x, int y)
    {
        int biomeIndex = System.Array.IndexOf(biomeCols, biomeMap.GetPixel(x, y));
        if (biomeIndex >= 0)
            return biomes[biomeIndex];
        return null;
    }

    public void GenerateTerrain()
    {
        IGenerationObject tileSprites;
        for (int x = 0; x < terrainSize; x++)
        {
            float height;

            for (int y = 0; y < terrainSize; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                float perlinHeight = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * curBiome.heightMultiplier + heightAddition;
                // Smooth transition into flat area
                if (x >= flatAreaStartX - transitionWidth && x < flatAreaStartX)
                {
                    float t = (x - (flatAreaStartX - transitionWidth)) / (float)transitionWidth;
                    height = Mathf.Lerp(perlinHeight, flatHeightStart, t);
                }
                // Smooth transition out of flat area
                else if (x > flatAreaEndX && x <= flatAreaEndX + transitionWidth)
                {
                    float t = (x - flatAreaEndX) / (float)transitionWidth;
                    height = Mathf.Lerp(flatHeightStart, perlinHeight, t);
                }
                // Flat area
                else if (x >= flatAreaStartX && x <= flatAreaEndX)
                {
                    height = flatHeightStart;
                }
                // Normal terrain
                else
                {
                    height = perlinHeight;
                }

                if (y >= height)
                {
                    break;
                }

                if (y < height - curBiome.dirtLayerHeight)
                {
                    tileSprites = curBiome.tileAtlas.stone;
                    /*
                    //ore and stone generation
                    if (ores[0].spreadTexture.GetPixel(x,y).r > 0.5f && height - y > ores[0].masSpawnHeight)
                        tileSprites = tileAtlas.coal;
                    if (ores[1].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[1].masSpawnHeight)
                        tileSprites = tileAtlas.iron;
                    if (ores[2].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[2].masSpawnHeight)
                        tileSprites = tileAtlas.gold;*/
                }
                else if (y < height - 1)
                {
                    tileSprites = curBiome.tileAtlas.dirt;
                }
                else
                {
                    //top later of the terrain
                    tileSprites = curBiome.tileAtlas.grass;
                }

                if (generateCave)
                {
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f || (x >= flatAreaStartX && x <= flatAreaEndX && y <= flatHeightStart && y >= flatHeightEnd))
                    {
                        PlaceTile(tileSprites, x, y);
                    }
                    else if (tileSprites.NeedsBackground)
                    {
                        PlaceWallTile(tileSprites, x, y);
                    }
                }
                else
                {
                    PlaceTile(tileSprites, x, y);
                }

                //tree
                if (y >= height - 1)
                {
                    int t = Random.Range(0, curBiome.treeChance);
                    if (t == 1)
                    {
                        //generate a tree
                        if (worldTilesDictionary.ContainsKey(new Vector2Int(x, y)))
                        {
                            GenerateTree(x, y + 1);
                        }
                    }
                    else
                    {
                        int i = Random.Range(0, curBiome.addonsChance);
                        //generate natural stuff like flowers and tall grass

                        if (worldTilesDictionary.ContainsKey(new Vector2Int(x, y)) && i == 1)
                        {
                            if (curBiome.tileAtlas.natureAddons != null)
                                PlaceTile(curBiome.tileAtlas.natureAddons, x, y + 1);
                        }
                    }
                }
            }
        }

        currentTerrain = worldTilesDictionary;
    }

    public void AddCore()
    {
        // Calculate the middle X-coordinate of the flat area
        int midX = (flatAreaStartX + flatAreaEndX) / 2;

        // Create the position Vector2
        Vector2 corePosition = new Vector2(midX + 0.5f, flatHeightStart + 1.67f);

        // Instantiate the coreArchPrefab at the calculated position
        GameObject coreArch = Instantiate(coreArchPrefab, corePosition, Quaternion.identity);

        PlaceGameObjectAfter(coreArch, corePosition);
    }


    public void GenerateNoiseTextures(float frequency, float limit, Texture2D noiseTexture)
    {
        float v;
        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                v = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);
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

    private void PlaceTile(IGenerationObject tile, int x, int y)
    {
        if (!worldTilesDictionary.ContainsKey(new Vector2Int(x, y)))
        {
            //var tileGameObject = tile.GetGeneratedGameObjects();
            int chunkCoord = Mathf.RoundToInt(Mathf.Round(x / chunkSize) * chunkSize);
            chunkCoord /= chunkSize;

            //tileGameObject.transform.parent = worldChunks[chunkCoord].transform;
            //tileGameObject.transform.position = new Vector2(x + 0.5f, y + 0.5f);
            //worldTilesDictionary.Add(new Vector2Int(x, y), tileGameObject);
        }
    }

    public void PlaceWallTile(IGenerationObject tile, int x, int y)
    {
        if (!worldTilesDictionary.ContainsKey(new Vector2Int(x, y)))
        {
            //var tileGameObject = tile.GetGeneratedWallGameObjects();
            int chunkCoord = Mathf.RoundToInt(Mathf.Round(x / chunkSize) * chunkSize);
            chunkCoord /= chunkSize;

            //tileGameObject.transform.parent = worldChunks[chunkCoord].transform;
            //tileGameObject.transform.position = new Vector2(x + 0.5f, y + 0.5f);
        }
    }

    public bool PlaceTileGameObject(GameObject gameObject, Vector2 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        if (!worldTilesDictionary.ContainsKey(new Vector2Int(x, y)))
        {
            int chunkCoord = Mathf.RoundToInt(Mathf.Round(x / chunkSize) * chunkSize);
            chunkCoord /= chunkSize;
            gameObject.transform.parent = worldChunks[chunkCoord].transform;
            worldTilesDictionary.Add(new Vector2Int(x, y), gameObject);
            return true;
        }

        return false;
    }

    public void PlaceGameObjectAfter(GameObject gameObject, Vector2 position) //after terrain generation
    {
        int x = (int)position.x;
        int y = (int)position.y;
        if (!worldTilesDictionary.ContainsKey(new Vector2Int(x, y)))
        {
            int chunkCoord = Mathf.RoundToInt(Mathf.Round(x / chunkSize) * chunkSize);
            chunkCoord /= chunkSize;
            gameObject.transform.parent = worldChunks[chunkCoord].transform;
        }
    }

    //shadow old code

    #region

    public static void ShadowUpdate()
    {
        int count = 0;

        for (float x = PlayerPosition.x - 13f; x < PlayerPosition.x + 13f; x += 0.25f)
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

    #endregion
}