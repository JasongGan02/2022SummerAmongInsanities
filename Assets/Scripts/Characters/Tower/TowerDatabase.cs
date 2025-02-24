using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Tower Database")]
public class TowerDatabase : ScriptableObject
{
    public List<TowerObject> availableTowers;
}