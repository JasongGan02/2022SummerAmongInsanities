using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileObjectRegistry", menuName = "Registry/TileObjectRegistry")]
public class TileObjectRegistry : ScriptableObject
{
    [SerializeField]
    private List<TileObject> tileObjects;

    private static ConcurrentDictionary<int, TileObject> tileObjectMap;


    public void Initialize()
    {
        tileObjectMap = new ConcurrentDictionary<int, TileObject>();
        foreach (var tileObject in tileObjects)
        {
            if (tileObject != null)
            {
                tileObjectMap[tileObject.TileID] = tileObject;
            }
        }
    }
    public static TileObject GetTileObjectByID(int id)
    {
        if (tileObjectMap != null)
        {
            if (tileObjectMap.TryGetValue(id, out TileObject tileObject))
            {
                return tileObject;
            }
            else
            {
                Debug.LogWarning($"TileObject with ID {id} not found.");
                return null;
            }
        }
        else
        {
            Debug.LogError("tileObjectMap is null");
        }
        return null;

    }
}
