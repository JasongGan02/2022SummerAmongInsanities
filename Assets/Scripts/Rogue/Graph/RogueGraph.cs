using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Rogue Graph", menuName = "Rogue/Graph")]
public class RogueGraph : ScriptableObject
{
    public List<RogueGraphNode> nodes = new();
    public RogueGraphNode rootNode = null;

    private GUIStyle normalStyle;

    private void OnEnable()
    {
        normalStyle = new GUIStyle();
        normalStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        normalStyle.normal.textColor = Color.white;
        normalStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        normalStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
    }

    public void DrawNodes()
    {
        foreach (RogueGraphNode node in nodes)
        {
            node.Draw(normalStyle);
        }
    }

    public void DrawConnectionLines()
    {
        DrawConnectionLines(rootNode, new HashSet<RogueGraphNode>());
    }

    private void DrawConnectionLines(RogueGraphNode node, HashSet<RogueGraphNode> visited)
    {
        if (!visited.Contains(node) && node.childNodes.Count > 0)
        {
            foreach (RogueGraphNode childNode in node.childNodes)
            {
                node.DrawConnectionLine(childNode);
                visited.Add(node);
                DrawConnectionLines(childNode, visited);
            }
        }
    }

    public void DeleteNode(RogueGraphNode node)
    {
        nodes.Remove(node);
        foreach (RogueGraphNode parentNode in node.parentNodes)
        {
            parentNode.childNodes.Remove(node);
        }
        foreach (RogueGraphNode childNode in node.childNodes)
        {
            childNode.parentNodes.Remove(node);
        }
    }

    private const int nodePadding = 25;
    private const int nodeBorder = 12;
}
