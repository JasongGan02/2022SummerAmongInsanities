using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileObjectRegistry", menuName = "Registry/TileObjectRegistry")]
public class TileObjectRegistry : ScriptableObject
{
    [SerializeField]
    private List<TileObject> tileObjects;

    private static Dictionary<int, TileObject> tileObjectMap;

    public void OnEnable()
    {
        tileObjectMap = new Dictionary<int, TileObject>();
        foreach (var tileObject in tileObjects)
        {
            if (tileObject != null) 
                tileObjectMap[tileObject.TileID] = tileObject;
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
        Debug.LogWarning($"TileObjectMap not found.");
        return null;

    }
}
