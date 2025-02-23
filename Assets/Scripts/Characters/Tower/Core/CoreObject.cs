using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Character Objects/Tower Object/Core Object")]
public class CoreObject : TowerObject
{
    [SerializeField]
    private CoreStats coreStats;

    protected override void OnEnable()
    {
        baseStats = coreStats; // Ensure the baseStats is set
        CharacterOnEnable();
    }
}