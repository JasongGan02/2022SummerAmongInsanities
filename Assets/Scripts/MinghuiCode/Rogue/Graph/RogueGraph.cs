using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Rogue Graph", menuName = "Rogue/Graph")]
public class RogueGraph : ScriptableObject
{
    public List<RogueGraphNode> nodes;
}
