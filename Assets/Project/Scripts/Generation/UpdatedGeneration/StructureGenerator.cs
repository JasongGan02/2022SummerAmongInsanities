using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureGenerator : MonoBehaviour
{
    [System.Serializable]
    public class StructureBlockInfo
    {
        public Vector2Int offsetFromOPoint;
        public int typeToAssign;
    };

    [SerializeField] private StructureBlockInfo[] StructureInfo;

    [Range(0f, 1f)]
    [SerializeField] private float genThreshold;
    private System.Random randomGen;

    private void Awake()
    {
        randomGen = new System.Random(1337);
    }

    private void applyStructure(ref int[,] dataToModify, int originCoord, int x, int y)
    {
        for (int i = 0; i < StructureInfo.Length; i++)
        {
            StructureBlockInfo info = StructureInfo[i];
            Vector2Int p = new Vector2Int
            {
                x = x + info.offsetFromOPoint.x,
                y = y + info.offsetFromOPoint.y
            };

            try
            {
                dataToModify[p.x, p.y] = info.typeToAssign;
            }
            catch (System.IndexOutOfRangeException)
            {
                int worldX = p.x + (originCoord * WorldGenerator.ChunkSize.x);
                int worldY = p.y;
                Vector2Int pos = new Vector2Int(worldX, worldY);

                int newCoords = WorldGenerator.GetChunkCoordsFromPosition(pos);
                Vector2Int chunkCoords = WorldGenerator.WorldToLocalCoords(pos, newCoords);

                if (WorldGenerator.AdditiveWorldData.ContainsKey(newCoords))
                {
                    WorldGenerator.AdditiveWorldData[newCoords][chunkCoords.x, chunkCoords.y] = info.typeToAssign;
                }
                else
                {
                    int[,] emptyData = new int[WorldGenerator.ChunkSize.x, WorldGenerator.ChunkSize.y];
                    emptyData[chunkCoords.x, chunkCoords.y] = info.typeToAssign;
                    WorldGenerator.AdditiveWorldData.Add(newCoords, emptyData);
                }
            }
        }
    }

    private int getTopBlockFromDataXZ(int[,] data, int x)
    {
        for (int y = WorldGenerator.ChunkSize.y - 1; y >= 0; y--)
        {
            if (data[x, y] != 0)
            {
                return y;
            }
        }

        return -1;
    }

    public void GenerateStructure(int chunkCoord, ref int[,] dataToModify, int x, int z)
    {
        float randomValue = (float)randomGen.NextDouble();
        if (randomValue >= genThreshold)
        {
            applyStructure(ref dataToModify, chunkCoord, x, getTopBlockFromDataXZ(dataToModify, x));
        }
    }
}