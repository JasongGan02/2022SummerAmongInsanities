using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Soul")]
public class SoulObject : BaseObject
{
    public float soulUnitValue = 10; // How much each Soul orb is worth
    
    // Method to spawn a number of Soul objects based on soulCount
    public void GetDroppedSoul(float soulValue, Vector3 dropPosition)
    {
        int soulCount = (int) (soulValue / soulUnitValue);
        for (int i = 0; i < soulCount; i++)
        {
            Vector3 randomOffset = Random.insideUnitCircle * 0.5f; // Randomize the drop position a little
            var soulObject = Instantiate(prefab, dropPosition + randomOffset, Quaternion.identity);
            var controller = soulObject.AddComponent<SoulController>();
            controller.Initialize(soulValue/soulUnitValue);
        }
        
    }
}
