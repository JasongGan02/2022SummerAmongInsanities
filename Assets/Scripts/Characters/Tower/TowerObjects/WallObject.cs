using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Character Objects/Tower Object/Wall Object")]
public class WallObject : TowerObject
{
    [Header("WallObject Fields")] 
    public TowerStats wallStats;
    public List<EffectObject> onHitEffects = new List<EffectObject>();
    
    protected override void OnEnable()
    {
        baseStats = wallStats;  // Ensure the baseStats is set
        CharacterOnEnable();
    }
    
}
