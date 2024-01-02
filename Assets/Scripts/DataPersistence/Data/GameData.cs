using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{   
    public long lastUpdated;
    public int deathCount;
    public Vector3 playerPosition;
    public SerializableWorldData serializableWorldData;

    //the values defined in this constructor will be the default values 
    //the game starts with when there is no data to load
    public GameData()
    {
        this.deathCount = 0;
        this.playerPosition = Vector3.zero;
    }

    public static SerializableWorldData SetWorldData(Dictionary<Vector2Int, int[,]> dictionary)
    {
        var serializableWorldData = new SerializableWorldData();
        foreach (var kvp in dictionary)
        {
            serializableWorldData.pairs.Add(new SerializableKeyValuePair { chunkCoord = kvp.Key, chunkData = Flatten2DArray(kvp.Value) });
        }
        return serializableWorldData;
    }
    public Dictionary<Vector2Int, int[,]> GetWorldData()
    {
        // Convert and return from worldData
        if (serializableWorldData != null) 
        {
            Dictionary<Vector2Int, int[,]> worldData = new Dictionary<Vector2Int, int[,]>();
            foreach (SerializableKeyValuePair serializableKeyValuePair in serializableWorldData.pairs)
            {
                worldData[serializableKeyValuePair.chunkCoord] = Unflatten2DArray(serializableKeyValuePair.chunkData, WorldGenerator.ChunkSize.x, WorldGenerator.ChunkSize.y);
            }
            return worldData;
        }
        return null;
    }

    public static int[] Flatten2DArray(int[,] array)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);
        int[] flatArray = new int[rows * cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                flatArray[i * cols + j] = array[i, j];
            }
        }
        return flatArray;
    }

    public int[,] Unflatten2DArray(int[] flatArray, int rows, int cols)
    {
        int[,] array = new int[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                array[i, j] = flatArray[i * cols + j];
            }
        }
        return array;
    }
}


[System.Serializable]
public class SerializableKeyValuePair
{
    public Vector2Int chunkCoord;
    public int[] chunkData;
}

[System.Serializable]
public class SerializableWorldData
{
    public List<SerializableKeyValuePair> pairs = new List<SerializableKeyValuePair>();
}

