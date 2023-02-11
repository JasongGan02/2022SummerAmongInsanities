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

    public RogueGraphNode CreateRootNode()
    {
        return CreateNode(new Vector2(200f, 200f), true);
    }

    public RogueGraphNode CreateNode(Vector2 position, bool isRoot = false)
    {
        RogueGraphNode node = ScriptableObject.CreateInstance<RogueGraphNode>();
        node.Init(
            new Rect(position, new Vector2(nodeWidth, nodeHeight)),
            currentGraph,
            isRoot
            );
        currentGraph.nodes.Add(node);

        AssetDatabase.AddObjectToAsset(node, currentGraph);
        AssetDatabase.SaveAssets();

        return node;
    }

    private const float nodeWidth = 200f;
    private const float nodeHeight = 100f;
}
