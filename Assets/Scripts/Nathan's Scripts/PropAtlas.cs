using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PropAtlas", menuName = "Atlas/Prop Atlas")]
public class PropAtlas : ScriptableObject
{
    [Header("Props")]
    public MedicineObject Health_Potion;
    public FireTorchObject Fire_Torch;
}
