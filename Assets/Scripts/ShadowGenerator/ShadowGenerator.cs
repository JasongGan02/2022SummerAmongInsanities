using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShadowGenerator : MonoBehaviour
{
    // in world space, the width of a block is 0.25. The first block's x is 0.13
    // position to x index should be => (position.x - 0.13) / 0.25

    // in parent object's space, the width of a block is 1. The first block's x is 0.5
    // position to x index should be => position.x - 0.5 or (int) position.x
    public int lightRadius;
    [Tooltip("the height of top tile + 1")]

    private Dictionary<Vector2Int, GameObject> worldTilesDictionary = null;
    private GameObject player;
    private int worldWidthInBlock;
    private int worldHeightInBlock = 100; // TODO don't hardcode
    //private int worldHeightInBlock = 80;
    TerrainGeneration terrainGeneration;
    Light2D GlobalLight;



    public Texture2D lightMap;
    public Material lightShader;
    private float[,] lightValues;
    [SerializeField] private int iterations;
    [Tooltip("between 0 & 15")][SerializeField] private float sunlightBrightness;
    [SerializeField] private float LightFade;
    [SerializeField] private float DarkLevel;
    [SerializeField] private Transform lightMapOverlay;
    private int dirtHeight;

    private void Start()
    {
        player = GameObject.Find(Constants.Name.PLAYER);
        GlobalLight = GameObject.Find("BackgroundLight").GetComponent<Light2D>();
    }

    private void FixedUpdate()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player");
        else
        {
            if (Vector2.Distance(player.transform.position, this.transform.position) > 1)
            {
                this.transform.position = player.transform.position;
            }
        }
    }

    public void Initialize(Dictionary<Vector2Int, GameObject> dictionary, int worldWidth, int dirtHeight)
    {
        worldTilesDictionary = dictionary;
        worldWidthInBlock = worldWidth;
        //worldWidthInBlock = 60;
        this.dirtHeight = dirtHeight;
        worldHeightInBlock = worldWidth / 2 + 6 + dirtHeight;
        //worldHeightInBlock = 1000;
        Debug.Log(worldWidthInBlock + " " + worldHeightInBlock);

        lightMapOverlay.localScale = new Vector3(43, 26, 1);
        lightValues = new float[worldWidthInBlock, worldHeightInBlock];
        lightMap = new Texture2D(worldWidthInBlock, worldHeightInBlock);
        lightShader.SetTexture("_ShadowTex", lightMap);

        lightMap.filterMode = FilterMode.Point; //< remove this line for smooth lighting, keep it for tiled lighting
        lightMap.wrapMode = TextureWrapMode.Clamp;
    }

    public void IUpdate() //call this method for any lighting updates
    {
        StopCoroutine(UpdateLighting());
        StartCoroutine(UpdateLighting());
    }


    private IEnumerator UpdateLighting() //calculate the new light values for every tile in the world
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < iterations; i++)
        {
            for (int x = 0; x < worldWidthInBlock; x++)
            {
                float lightLevel = sunlightBrightness;
                for (int y = worldHeightInBlock - 1; y >= 0; y--)
                {
                    if (x < 60 || x > 119 || y < 2) lightLevel = sunlightBrightness;
                    else if (IsBackground(x, y) && !IsUnderGround(x, y)) //if illuminate block
                    //if (!IsCovered(x, y))
                    {
                        if (!worldTilesDictionary.ContainsKey(new Vector2Int(x, y)))
                        {
                            lightLevel = sunlightBrightness;
                        }
                        else
                        {
                            //Debug.Log(worldTilesDictionary[new Vector2Int(x, y)]);
                            lightLevel = sunlightBrightness;
                        }
                    }
                    else
                    {
                        //find brightest neighbour
                        int nx1 = Mathf.Clamp(x - 1, 0, worldWidthInBlock - 1);
                        int nx2 = Mathf.Clamp(x + 1, 0, worldWidthInBlock - 1);
                        int ny1 = Mathf.Clamp(y - 1, 0, worldHeightInBlock - 1);
                        int ny2 = Mathf.Clamp(y + 1, 0, worldHeightInBlock - 1);

                        lightLevel = Mathf.Max(
                            lightValues[nx1, y],
                            lightValues[nx2, y],
                            lightValues[x, ny1],
                            lightValues[x, ny2]);

                        lightLevel -= LightFade;
                        if (lightLevel <= DarkLevel) { lightLevel = 0; }
                    }

                    lightValues[x, y] = lightLevel;
                }
            }

            for (int x = worldWidthInBlock - 1; x >= 0; x--)
            {
                float lightLevel = sunlightBrightness;
                for (int y = 0; y < worldHeightInBlock; y++)
                {
                    if (x < 60 || x > 119 || y < 2) lightLevel = sunlightBrightness;
                    if (IsBackground(x, y) && !IsUnderGround(x, y)) //if illuminate block
                    {
                        lightLevel = sunlightBrightness;
                    }
                    else
                    {
                        //find brightest neighbour
                        int nx1 = Mathf.Clamp(x - 1, 0, worldWidthInBlock - 1);
                        int nx2 = Mathf.Clamp(x + 1, 0, worldWidthInBlock - 1);
                        int ny1 = Mathf.Clamp(y - 1, 0, worldHeightInBlock - 1);
                        int ny2 = Mathf.Clamp(y + 1, 0, worldHeightInBlock - 1);

                        lightLevel = Mathf.Max(
                            lightValues[nx1, y],
                            lightValues[nx2, y],
                            lightValues[x, ny1],
                            lightValues[x, ny2]);

                        lightLevel -= LightFade;
                        if (lightLevel <= DarkLevel) { lightLevel = 0; }
                    }

                    lightValues[x, y] = lightLevel;
                }
            }
        }

        //apply to texture
        for (int x = 0; x < worldWidthInBlock; x++)
        {
            for (int y = 0; y < worldHeightInBlock; y++)
            {
                lightMap.SetPixel(x, y, new Color(0, 0, 0, 1 - (lightValues[x, y] / sunlightBrightness)));
            }
        }
        lightMap.Apply(); //send new texture to the GPU so that we can render it on screen
    }

    private void LightBlocks()
    {
        for (int x = 0; x < worldWidthInBlock; x++)
        {
            for (int y = 0; y < worldHeightInBlock; y++)
            {
                if (lightMap.GetPixel(x, y).a == 0f) // this pixel is completely transparent
                {
                    LightBlock(x, y, 1f, 0);
                }
            }
        }
    }

    private void LightBlock(int x, int y, float intensity, float iteration)
    {
        if (iteration < lightRadius)
        {
            lightMap.SetPixel(x, y, new Color(0, 0, 0, 1 - intensity));

            for (int nx = x - 1; nx < x + 2; nx++)
            {
                for (int ny = y - 1; ny < y + 2; ny++)
                {
                    if (!(nx == x && ny == y))
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(nx, ny));
                        float targetIntensity = Mathf.Pow(0.5f, dist) * intensity;
                        Color targetTile = lightMap.GetPixel(nx, ny);
                        if (targetTile != null && targetTile.a > (1 - targetIntensity))
                        {
                            LightBlock(nx, ny, targetIntensity, iteration + 1);
                        }
                    }
                }
            }
        }

        lightMap.Apply();
    }

    private void SnapOverlayToPlayer()
    {
        float blockX = 0;
        switch (player.transform.position.x % 1)
        {
            case float res when (res >= 0.25 && res < 0.5):
                blockX = (int)player.transform.position.x + 0.38f;
                break;
            case float res when (res >= 0.5 && res < 0.75):
                blockX = (int)player.transform.position.x + 0.63f;
                break;
            case float res when (res >= 0.75 && res < 1.0):
                blockX = (int)player.transform.position.x + 0.88f;
                break;
            case float res when (res >= 0.0 && res < 0.25):
                blockX = (int)player.transform.position.x + 0.13f;
                break;
        }

        this.transform.position = new Vector3(blockX, 0 + this.transform.position.y, 1);
    }

    public bool IsCovered(int x, int y)
    {
        if (worldTilesDictionary.ContainsKey(new Vector2Int(x - 1, y)) &&
            worldTilesDictionary.ContainsKey(new Vector2Int(x + 1, y)) &&
            worldTilesDictionary.ContainsKey(new Vector2Int(x, y - 1)) &&
            worldTilesDictionary.ContainsKey(new Vector2Int(x, y + 1)))
        {
            return true;
        }

        return false;
    }

    public bool IsUnderGround(int x, int y)
    {
        if (x != 1 && x != worldWidthInBlock - 1 && y != 1 && y != worldHeightInBlock - 1) // skip Map Boundary Tile
        {
            for (int yy = y + 1; yy < worldHeightInBlock; yy++)
            {
                if (worldTilesDictionary.ContainsKey(new Vector2Int(x, yy)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void PlayerAutoCleanShadow()
    {
        int cleaningRadius = 2; // Adjust this value as needed
        float playerX = player.transform.position.x * 4;
        float playerY = player.transform.position.y * 4;

        for (int x = (int)playerX - cleaningRadius; x <= playerX + cleaningRadius; x++)
        {
            for (int y = (int)playerY - cleaningRadius; y <= playerY + cleaningRadius; y++)
            {
                if (x >= 0 && x < worldWidthInBlock && y >= 0 && y < worldHeightInBlock)
                {
                    if (lightMap.GetPixel(x, y) != Color.white && lightMap.GetPixel(x, y).a > 0f)
                    {
                        Debug.Log("Clean position: " + x + "," + y);
                        LightBlock(x, y, 0.8f, 0.2f);
                    }
                }
            }
        }
    }

    public void LightAutoCleanShadow(float a, float b, float cleanRadius)
    {
        float propX = a * 4f;
        float propY = b * 4f;

        for (int x = (int)(propX - cleanRadius); x <= propX + cleanRadius; x++)
        {
            for (int y = (int)(propY - cleanRadius); y <= propY + cleanRadius; y++)
            {
                if (x >= 0 && x < worldWidthInBlock && y >= 0 && y < worldHeightInBlock)
                {
                    float distanceSquared = (x - propX) * (x - propX) + (y - propY) * (y - propY);
                    if (distanceSquared <= cleanRadius * cleanRadius)
                    {
                        if (lightMap.GetPixel(x, y) != Color.white && lightMap.GetPixel(x, y).a > 0f)
                        {
                            LightBlock(x, y, 0.8f, 0.2f);
                        }
                    }
                }
            }
        }
    }

    public bool IsLightCovered(int x, int y)
    {
        if (GlobalLight.intensity < 0.2f)
        {
            return true;
        }

        Color pixelColor = lightMap.GetPixel(x, y);
        float intensity = 0.299f * pixelColor.r + 0.587f * pixelColor.g + 0.114f * pixelColor.b;
        if (intensity > 0.3f)
        {
            return true;
        }

        float playerX = player.transform.position.x * 4;
        float playerY = player.transform.position.y * 4;
        float distanceToPlayer = (x - playerX) * (x - playerX) + (y - playerY) * (y - playerY);
        if (distanceToPlayer < 9)
        {
            return true;
        }

        return false;
    }

    public bool IsBackground(int x, int y)
    {
        if (worldTilesDictionary.ContainsKey(new Vector2Int(x, y)))
        {
            return false;
        }
        return true;
    }

    public void OnTileBrokenOriginal(Vector2Int coord)
    {
        if (worldTilesDictionary.ContainsKey(coord))
        {
            worldTilesDictionary.Remove(coord);
            if (coord.y >= worldHeightInBlock - worldWidthInBlock)
            {
                // lightMap.SetPixel(coord.x, coord.y, Color.clear);

                for (int y = worldHeightInBlock - 1; y >= worldHeightInBlock - worldWidthInBlock; y--)
                {
                    if (worldTilesDictionary.ContainsKey(new Vector2Int(coord.x, y)))
                    {
                        break;
                    }
                    else
                    {
                        // TODO the intensity should be related to skyLightHeight - y
                        LightBlock(coord.x, 1, 1f, 0);
                    }
                }
            }
        }
    }

    public void OnTileBroken(Vector2Int coord)
    {
        if (worldTilesDictionary.ContainsKey(coord))
        {
            if (coord.y >= worldHeightInBlock - worldWidthInBlock)
            {
                for (int y = worldHeightInBlock - 1; y >= worldHeightInBlock - worldWidthInBlock; y--)
                {
                    if (worldTilesDictionary.ContainsKey(new Vector2Int(coord.x, y)))
                    {
                        break;
                    }
                    else
                    {
                        float intensity = Mathf.Clamp01((worldHeightInBlock - y) / (float)worldWidthInBlock);
                        float transparency = Mathf.Clamp01(1 - intensity);
                        LightBlock(coord.x, y, transparency, intensity);
                    }
                }
            }

            worldTilesDictionary.Remove(coord);
        }
    }

    public void TerrainShadowUpdate(int worldWidth)
    {
        terrainGeneration = FindObjectOfType<TerrainGeneration>();
        if (terrainGeneration != null)
        {
            worldTilesDictionary = terrainGeneration.currentTerrain;
        }
        Color[] colors = new Color[worldWidth * worldHeightInBlock];

        for (int x = 0; x < worldWidth; x++)
        {
            bool isExposedToSky = true; // Flag to track if the current position is exposed to the sky

            for (int y = worldHeightInBlock - 1; y >= worldHeightInBlock - worldWidthInBlock; y--)
            {
                if (worldTilesDictionary.ContainsKey(new Vector2Int(x, y)))
                {
                    isExposedToSky = false; // Found an obstruction, mark it as not exposed to the sky
                    break;
                }
            }

            if (isExposedToSky)
            {
                for (int y = worldHeightInBlock - 1; y >= worldHeightInBlock - worldWidthInBlock; y--)
                {
                    colors[y * worldWidth + x] = Color.clear;
                }
            }
        }
    }
}
