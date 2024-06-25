using UnityEngine;

public static class TileHelper
{
    private static TileObject GetTileAtPosition(Vector2Int worldPosition, int layer)
    {
        int chunkCoordX = WorldGenerator.GetChunkCoordsFromPosition(worldPosition);
        Vector2Int chunkCoord = new Vector2Int(chunkCoordX, 0);
        Vector2Int localPosition = WorldGenerator.WorldToLocalCoords(worldPosition, chunkCoordX);

        if (WorldGenerator.WorldData.ContainsKey(chunkCoord))
        {
            TileObject[,,] chunkData = WorldGenerator.WorldData[chunkCoord];
            if (localPosition.x >= 0 && localPosition.x < chunkData.GetLength(0) &&
                localPosition.y >= 0 && localPosition.y < chunkData.GetLength(1) &&
                layer >= 0 && layer < chunkData.GetLength(2))
            {
                return chunkData[localPosition.x, localPosition.y, layer];
            }
        }

        return null;
    }
    private static bool IsSpecifiedTile(TileObject tile, TileObject[] specifiedTiles)
    {
        foreach (var specifiedTile in specifiedTiles)
        {
            if (tile == specifiedTile)
            {
                return true;
            }
        }
        return false;
    }
    public static (int spriteNumber, Quaternion rotation) GetSpriteNumberAndRotation(Vector2Int worldPosition, TileObject[] specifiedTiles)
    {
        
        Vector2Int abovePosition = new Vector2Int(worldPosition.x, worldPosition.y + 1);
        Vector2Int belowPosition = new Vector2Int(worldPosition.x, worldPosition.y - 1);
        Vector2Int leftPosition = new Vector2Int(worldPosition.x - 1, worldPosition.y);
        Vector2Int rightPosition = new Vector2Int(worldPosition.x + 1, worldPosition.y);


        bool hasAbove = false;
        bool hasBelow = false;
        bool hasLeft = false;
        bool hasRight = false;

        for (int layer = 0; layer < WorldGenerator.TileLayers; layer++)
            {
                hasAbove |= IsSpecifiedTile(GetTileAtPosition(abovePosition, layer), specifiedTiles);
                hasBelow |= IsSpecifiedTile(GetTileAtPosition(belowPosition, layer), specifiedTiles);
                hasLeft |= IsSpecifiedTile(GetTileAtPosition(leftPosition, layer), specifiedTiles);
                hasRight |= IsSpecifiedTile(GetTileAtPosition(rightPosition, layer), specifiedTiles);
            }



        int spriteNumber = 0;
        Quaternion rotation = Quaternion.identity;

        if (hasAbove && hasBelow && hasLeft && hasRight)
        {
            spriteNumber = 5;
        }
        else if (hasAbove && hasBelow && hasLeft)
        {
            spriteNumber = 4;
            rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (hasAbove && hasBelow && hasRight)
        {
            spriteNumber = 4;
            rotation = Quaternion.Euler(0, 0, -90);
        }
        else if (hasAbove && hasLeft && hasRight)
        {
            spriteNumber = 4;
        }
        else if (hasBelow && hasLeft && hasRight)
        {
            spriteNumber = 4;
            rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (hasAbove && hasBelow)
        {
            spriteNumber = 3;
            
        }
        else if (hasLeft && hasRight)
        {
            spriteNumber = 3;
            rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (hasAbove && hasRight)
        {
            spriteNumber = 2;
            
        }
        else if (hasAbove && hasLeft)
        {
            spriteNumber = 2;
            rotation = Quaternion.Euler(0, 0, 90);
            
        }
        else if (hasBelow && hasRight)
        {
            spriteNumber = 2;
            rotation = Quaternion.Euler(0, 0, -90);
            
        }
        else if (hasBelow && hasLeft)
        {
            spriteNumber = 2;
            rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (hasAbove)
        {
            spriteNumber = 1;
        }
        else if (hasRight)
        {
            spriteNumber = 1;
            rotation = Quaternion.Euler(0, 0, -90);
        }
        else if (hasBelow)
        {
            spriteNumber = 1;
            rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (hasLeft)
        {
            spriteNumber = 1;
            rotation = Quaternion.Euler(0, 0, 90);
        }

        return (spriteNumber, rotation);
    }


    public static (int spriteNumber, Quaternion rotation, bool flipX) GetGrassTileSpriteAndRotation(Vector2Int worldPosition, TileObject[] specifiedTiles)
    {
        Vector2Int abovePosition = new Vector2Int(worldPosition.x, worldPosition.y + 1);
        Vector2Int belowPosition = new Vector2Int(worldPosition.x, worldPosition.y - 1);
        Vector2Int leftPosition = new Vector2Int(worldPosition.x - 1, worldPosition.y);
        Vector2Int rightPosition = new Vector2Int(worldPosition.x + 1, worldPosition.y);

        bool hasAbove = IsSpecifiedTile(GetTileAtPosition(abovePosition, 1), specifiedTiles);
        bool hasBelow = IsSpecifiedTile(GetTileAtPosition(belowPosition, 1), specifiedTiles);
        bool hasLeft = IsSpecifiedTile(GetTileAtPosition(leftPosition, 1), specifiedTiles);
        bool hasRight = IsSpecifiedTile(GetTileAtPosition(rightPosition, 1), specifiedTiles);


        int spriteNumber = 0;
        Quaternion rotation = Quaternion.identity;
        bool flipX = false;

        if(hasLeft && hasBelow && hasRight)
        {
            spriteNumber = Random.Range(0, 4);
        }
        else if (hasLeft && hasBelow && hasAbove) 
        {
            spriteNumber = 4;
        }
        else if(hasRight && hasBelow && hasAbove)
        {
            spriteNumber = 4;
            flipX = true;
        }
        else if(hasLeft && hasBelow)
        {
            spriteNumber = Random.Range(5, 7);
        }
        else if(hasRight && hasBelow)
        {
            spriteNumber = Random.Range(5, 7);
            flipX = true;
        }
        

        return (spriteNumber, rotation,flipX);
    }
}
