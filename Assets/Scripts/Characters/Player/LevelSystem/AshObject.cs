using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DivinityFrag", menuName = "Objects/Divinity Frag Object")]
public class AshObject : BaseObject
{
    public float ashUnitValue = 10; // How much each Soul orb is worth
    

    public GameObject GetDroppedGameObject(float ashValue, Vector3 dropPosition)
    {
        Vector3 randomOffset = Random.insideUnitCircle * 0.5f; // Randomize the drop position a little
        var ashObject = Instantiate(prefab, dropPosition + randomOffset, Quaternion.identity);
        var controller = ashObject.AddComponent<AshController>();
        controller.Initialize(ashValue);
        return ashObject;
    }
}
