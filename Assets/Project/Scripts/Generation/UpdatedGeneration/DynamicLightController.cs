using System;
using UnityEngine;

public class DynamicLightController : MonoBehaviour
{
    private Vector2Int prevChunkCoord;
    private Vector2Int prevWorldPosition;
    private TileObject dynamicLightTile;
    private int lightLayer = 4; // 第五层
    
    public void Initialize(TileObject tileObject, Vector2 position)
    {
        dynamicLightTile = tileObject;
        UpdateLightPosition(position);
    }

    private void Update()
    {
        Vector2 newPostion = transform.position + new Vector3(0.01f, 0, 0);
        UpdateLightPosition(newPostion);

    }
    
    public void UpdateLightPosition(Vector2 newPosition)
    {
        // 更新 transform 的位置
        transform.position = newPosition;

        // 将世界坐标转为对应的 chunk 坐标
        Vector2Int newWorldPosition = new Vector2Int(Mathf.FloorToInt(newPosition.x), Mathf.FloorToInt(newPosition.y));
        int newChunkCoord = WorldGenerator.GetChunkCoordsFromPosition(newPosition);

        // 先清除之前 Chunk 中的光源数据
        RemoveLightFromPreviousPosition();
        
        // 如果光源位置发生了 Chunk 的跨越
        if (new Vector2Int(newChunkCoord, 0) != prevChunkCoord)
        {
            // 更新 Chunk 坐标
            prevChunkCoord = new Vector2Int(newChunkCoord, 0);
        }

        // 更新 WorldData 中的光源数据
        RefreshWorldData(newWorldPosition);

        // 更新当前光源的世界位置
        prevWorldPosition = newWorldPosition;
    }

    /// <summary>
    /// 从上一个位置移除光源数据
    /// </summary>
    private void RemoveLightFromPreviousPosition()
    {
        if (!WorldGenerator.WorldData.ContainsKey(prevChunkCoord))
            return;

        // 将之前的光源数据移除
        Vector2Int prevLocalCoords = WorldGenerator.WorldToLocalCoords(prevWorldPosition, prevChunkCoord.x);
        WorldGenerator.WorldData[prevChunkCoord][prevLocalCoords.x, prevLocalCoords.y, lightLayer] = null;
    }

    /// <summary>
    /// 更新 WorldData 中的光源信息
    /// </summary>
    /// <param name="worldPosition">当前光源的新世界位置</param>
    private void RefreshWorldData(Vector2Int worldPosition)
    {
        // 如果 WorldData 中还没有该 chunk 数据，则跳过
        if (!WorldGenerator.WorldData.ContainsKey(prevChunkCoord))
        {
            Debug.LogError("WorldData does not contain chunk at: " + prevChunkCoord);
            return;
        }

        // 计算光源在 chunk 内的本地坐标
        Vector2Int localCoords = WorldGenerator.WorldToLocalCoords(worldPosition, prevChunkCoord.x);

        // 更新 WorldData 中对应层的光源数据
        WorldGenerator.WorldData[prevChunkCoord][localCoords.x, localCoords.y, lightLayer] = dynamicLightTile;
        
        // 更新光源数据
        WorldGenerator.Instance.RefreshChunkLight(prevChunkCoord, true);
    }
    
    private void OnDrawGizmos()
    {
        // 可视化动态光源的位置（可选）
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
