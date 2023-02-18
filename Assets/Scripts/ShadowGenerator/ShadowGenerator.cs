using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowGenerator : MonoBehaviour
{
    // in world space, the width of a block is 0.25. The first block's x is 0.13
    // position to x index should be => (position.x - 0.13) / 0.25

    // in parent object's space, the width of a block is 1. The first block's x is 0.5
    // position to x index should be => position.x - 0.5 or (int) position.x
    public Texture2D lightMap;
    public Material lightShader;
    public int lightRadius = 4;
    public int thresholdToSnapOverlay = 20;
    [Tooltip("the height of top tile + 1")]
    public int skyLightHeight = 62;
    public int skyLightRange = 5;

    private Dictionary<Vector2Int, GameObject> worldTilesDictionary = null;
    private GameObject player;
    private int worldWidthInBlock;
    private int worldHeightInBlock = 100; // TODO don't hardcode

    private void Start()
    {
        player = GameObject.Find(Constants.Name.PLAYER);    
    }

    private void Update()
    {
        if(player == null)
            player = GameObject.FindWithTag("Player");
        else
        {
            float distanceX = Mathf.Abs(player.transform.position.x - transform.position.x) * 4;
            Debug.Log(distanceX);
            if (distanceX > thresholdToSnapOverlay)
            {
                SnapOverlayToPlayer();
            }
        }
        // distance is in global space, so needs to time 4 for global -> object conversion
        
    }

    public void Initialize(Dictionary<Vector2Int, GameObject> dictionary, int worldWidth)
    {
        worldTilesDictionary = dictionary;
        worldWidthInBlock = worldWidth;

        lightMap = new Texture2D(worldWidth, worldHeightInBlock);
        //lightMap.filterMode = FilterMode.Point;
        lightShader.SetTexture("_ShadowTex", lightMap);
        lightShader.SetFloat("_WorldWidth", worldWidth / 4);
        lightShader.SetFloat("_WorldHeight", worldHeightInBlock / 4);

        Color[] colors = new Color[worldWidth * worldHeightInBlock];

        int i = 0;

        // basic terrain shadow
        for (int y = 0; y < skyLightHeight; y++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                colors[i++] = Color.black;
            }
        }
        for (int y = skyLightHeight; y < worldHeightInBlock; y++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                colors[i++] = Color.clear;
            }
        }

        // add sky light
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = skyLightHeight - 1; y >= skyLightHeight - skyLightRange; y--)
            {
                if (worldTilesDictionary.ContainsKey(new Vector2Int(x, y)))
                {
                    break;
                }
                else
                {
                    colors[y * worldWidth + x] = Color.clear;
                }
            }
        }


        lightMap.SetPixels(colors);
        lightMap.Apply();

        LightBlocks();
    }

    private void LightBlocks()
    {
        for (int x = 0; x < worldWidthInBlock; x++)
        {
            for (int y = 0; y < worldHeightInBlock; y++)
            {
                if (lightMap.GetPixel(x, y).a == 0f)
                {
                    LightBlock(x, y, 1f, 0);
                }
            }
        }
    }

    private void LightBlock(int x, int y, float intensity, int iteration)
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
        switch(player.transform.position.x % 1)
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

        this.transform.position = new Vector3(blockX, 0 + this.transform.position.y, 0);
    }

    public void OnTileBroken(Vector2Int coord)
    {
        if (worldTilesDictionary.ContainsKey(coord))
        {
            worldTilesDictionary.Remove(coord);
            if (coord.y >= skyLightHeight - skyLightRange)
            {
                // lightMap.SetPixel(coord.x, coord.y, Color.clear);

                for (int y = skyLightHeight - 1; y >= skyLightHeight - skyLightRange; y--)
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
}
