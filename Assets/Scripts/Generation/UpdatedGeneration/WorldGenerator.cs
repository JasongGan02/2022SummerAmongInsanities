using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    public static Dictionary<Vector2Int, int[,]> WorldData;
    public static Dictionary<int, GameObject> ActiveChunks;
    public static Dictionary<int, GameObject> TotalChunks;
    public static Dictionary<int, int[,]> AdditiveWorldData;
    public static readonly Vector2Int ChunkSize = new Vector2Int(16, 256);
    private List<GameObject> WorldChunks;
    private DataGenerator dataCreator;
    public static float seed { get; private set; }
    public TerrainSettings[] settings;
    // Start is called before the first frame update
    void Start()
    {
        WorldData = new Dictionary<Vector2Int, int[,]>();
        ActiveChunks = new Dictionary<int, GameObject>();
        TotalChunks = new Dictionary<int, GameObject>();
        dataCreator = new DataGenerator(this, settings, GetComponent<StructureGenerator>());
        RecalculateSeed();
    }
    private void RecalculateSeed() { if (seed == 0) seed = Random.Range(-10000, 10000); }

    public IEnumerator CreateChunk(int ChunkCoord)
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
            TotalChunks.Add(ChunkCoord, newChunk);
            dataCreator.QueueDataToGenerate(new DataGenerator.GenData
            {
                GenerationPoint = pos,
                OnComplete = x => dataToApply = x
            });

            yield return new WaitUntil(() => dataToApply != null);
        }

        DrawChunk(dataToApply, pos);
    }

    public void DrawChunk(int[,] Data, Vector2Int offset)
    {
        for (int x = 0; x < ChunkSize.x; x++) 
        {
            for (int y = 0; y < ChunkSize.y; y++) 
            {
                int currentBlockID = Data[x, y];
                if (currentBlockID != 0)
                {
                    TileObject tileObject = TileObjectRegistry.GetTileObjectByID(Mathf.Abs(currentBlockID));//Background Wall would be negative ID
                    if (tileObject != null)
                    {
                        
                        PlaceTile(tileObject, x + (offset.x * ChunkSize.x), y, offset, currentBlockID < 0, false);
                    }
                    else
                    {
                        Debug.Log("no world");
                    }
                }
            }
        }
    }

    public static void PlaceTile(TileObject tile, int x, int y, Vector2Int chunkID, bool isWall, bool placeByPlayer, bool updateLighting = false) 
    {   //change to data needs to be done somewhere else
        if (y < 0 || y >= ChunkSize.y) return; //we only care about out of scope along y axis
        GameObject tileGameObject = placeByPlayer ? tile.GetPlacedGameObject() :
                                isWall ? tile.GetGeneratedWallGameObjects() :
                                tile.GetGeneratedGameObjects();
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
}
