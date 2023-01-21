using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RogueGraphNodeCreator
{
    public RogueGraph currentGraph;

    public RogueGraphNodeCreator(RogueGraph graph)
    {
        currentGraph = graph;
    }

    public void CreateRootNode()
    {
        CreateNode(new Vector2(200f, 200f));
    }

    public void CreateNode(Vector2 position)
    {
        RogueGraphNode node = ScriptableObject.CreateInstance<RogueGraphNode>();
        node.Init(
            new Rect(position, new Vector2(nodeWidth, nodeHeight)),
            currentGraph
            );
        currentGraph.nodes.Add(node);

        AssetDatabase.AddObjectToAsset(node, currentGraph);
        AssetDatabase.SaveAssets();
    }

    private const float nodeWidth = 200f;
    private const float nodeHeight = 100f;
}
