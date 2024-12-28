using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "usableobject", menuName = "Objects/Usable Object")]
public class UsableObject : DroppableObject
{
    [Header("UsableObject Fields")]
    public float pressDuration = 1.5f; // Time needed to press for the effect to start
    public EffectObject effect;
    
    public void UseEffect(IEffectableController effectableController)
    {
        effect.ExecuteEffect(effectableController);
    }
}