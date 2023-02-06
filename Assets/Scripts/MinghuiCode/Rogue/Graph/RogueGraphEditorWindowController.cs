using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RogueGraphEditorWindowController
{
    private RogueGraphNode headerClickedNode = null;
    public RogueGraphNode parentNode = null;

    public RogueGraph graph;
    private RogueGraphNodeCreator nodeCreator;

    public RogueGraphEditorWindowController(RogueGraph graph)
    {
        this.graph = graph;
        nodeCreator = new RogueGraphNodeCreator(this.graph);
    }

    public void ProcessEvent(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseUp:
                switch (e.button)
                {
                    case 0:
                        HandleLeftMouseUpEvent(e);
                        break;
                    case 1:
                        HandleRightMouseUpEvent(e);
                        break;
                }
                break;
            case EventType.MouseDown:
                switch (e.button)
                {
                    case 0:
                        HandleLeftMouseDownEvent(e);
                        break;
                    case 1:
                        HandleRightMouseDownEvent(e);
                        break;
                }
                break;
            case EventType.MouseDrag:
                switch (e.button)
                {
                    case 0:
                        HandleLeftMouseDragEvent(e);
                        break;
                    case 1:
                        HandleRightMouseDragEvent(e);
                        break;
                    case 2:
                        HandleMidMouseDragEvent(e);
                        break;
                }
                break;
        }
    }

    public void CreateRootNode()
    {
        if (graph.nodes.Count == 0)
        {
            graph.rootNode = nodeCreator.CreateRootNode();
            EditorUtility.SetDirty(graph);
        }
    }

    private void HandleRightMouseUpEvent(Event e)
    {
        RogueGraphNode node = GetNodeAtPosition(e.mousePosition);
        if (node != null)
        {
            if (node != graph.rootNode)
            {
                ShowNodeContextMenu(e.mousePosition, node);
            }
        }
        else
        {
            ShowGeneralContextMenu(e.mousePosition);
        }
    }

    private void HandleLeftMouseUpEvent(Event e)
    {
        ConnectNodesIfPossible(e);
     

        headerClickedNode = null;
        parentNode = null;
    }

    private void ConnectNodesIfPossible(Event e)
    {
        RogueGraphNode childNode = GetNodeAtPosition(e.mousePosition);
        if (parentNode != null && childNode != null && parentNode != childNode && !parentNode.HasAncester(childNode) &&
            !parentNode.childNodes.Contains(childNode))
        {
            parentNode.childNodes.Add(childNode);
            childNode.parentNodes.Add(parentNode);
            EditorUtility.SetDirty(childNode);
            EditorUtility.SetDirty(parentNode);
        }
    }

    private void HandleRightMouseDownEvent(Event e)
    {
        
    }

    private void HandleLeftMouseDownEvent(Event e)
    {
        RogueGraphNode nodeAtMousePosition = GetNodeAtPosition(e.mousePosition);

        if (CanMoveNode(e.mousePosition))
        {
            headerClickedNode = nodeAtMousePosition;
        }

        if (CanConnectNode(e.mousePosition))
        {
            parentNode = nodeAtMousePosition;
        }
    }

    private void HandleRightMouseDragEvent(Event e)
    {
        
    }

    private void HandleLeftMouseDragEvent(Event e)
    {
        if (headerClickedNode != null)
        {
            headerClickedNode.Move(e.delta);
        }
    }

    private void HandleMidMouseDragEvent(Event e)
    {
        foreach (RogueGraphNode node in graph?.nodes)
        {
            node.Move(e.delta);
        }
    }

    private void ShowGeneralContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new();
        menu.AddItem(new GUIContent("Create new node"), false, CreateNewRogueGraphNode, mousePosition);
        menu.ShowAsContext();
    }

    private void CreateNewRogueGraphNode(object obj)
    {
        Vector2 mousePosition = (Vector2)obj;
        if (mousePosition == null) return;

        nodeCreator.CreateNode(mousePosition);
    }

    private void ShowNodeContextMenu(Vector2 mousePosition, RogueGraphNode node)
    {
        GenericMenu menu = new();
        menu.AddItem(new GUIContent("Delete Node"), false, DeleteNode, node);
        menu.ShowAsContext();
    }

    private void DeleteNode(object obj)
    {
        RogueGraphNode node = (RogueGraphNode)obj;
        if (node == null) return;

        graph.DeleteNode(node);
    }

    private RogueGraphNode GetNodeAtPosition(Vector2 position)
    {
        foreach (RogueGraphNode node in graph.nodes)
        {
            if (node.IsMouseIn(position))
            {
                return node;
            }
        }
        return null;
    }

    private bool CanMoveNode(Vector2 mousePosition)
    {
        foreach (RogueGraphNode node in graph.nodes)
        {
            if (node.IsMouseInHeader(mousePosition))
            {
                return true;
            }
        }
        return false;
    }

    private bool CanConnectNode(Vector2 mousePosition)
    {
        foreach (RogueGraphNode node in graph.nodes)
        {
            if (node.IsMouseInBody(mousePosition))
            {
                return true;
            }
        }
        return false;
    }
}
