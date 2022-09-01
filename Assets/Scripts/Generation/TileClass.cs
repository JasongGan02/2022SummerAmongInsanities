using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName ="newtileclass", menuName = "Tile Class")]
public class TileClass : ScriptableObject
{
    public string tileName;
    public GameObject[] tilePrefabs;
    public int healthPoint;

    [System.Serializable]
    public class Drop
    {
        public CollectibleObject collectibleObject;
        public float chance; // 3.4 means you will certianly get 3 items and 40% chance to get 1 more.

        public int GetDroppedItemCount()
        {
            int count = (int)chance;
            if (!Mathf.Approximately(chance - count, 0) && Random.value > chance - count)
            {
                count += 1;
            }
            return count;
        }
    }

    public Drop[] drops;
}
