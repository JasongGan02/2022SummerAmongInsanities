using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RogueGraphEditorWindowController
{
    public HashSet<RogueGraphNode> selectedNodes = new();
    private RogueGraphNode headerClickedNode = null;
    private RogueGraphNode bodyClickedNode = null;
    private Vector2 dragOffest = Vector2.zero;

    public RogueGraph currentGraph;
    private RogueGraphNodeCreator nodeCreator;

    public Vector2? currentLineStartPoint = null;

    public RogueGraphEditorWindowController(RogueGraph graph)
    {
        currentGraph = graph;
        nodeCreator = new RogueGraphNodeCreator(currentGraph);
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
                }
                break;
        }
    }

    public void SetupWindow()
    {
        if (currentGraph.nodes.Count == 0)
        {
            nodeCreator.CreateRootNode();
        }
    }

    private void HandleRightMouseUpEvent(Event e)
    {
        ShowContextMenu(e.mousePosition);
    }

    private void HandleLeftMouseUpEvent(Event e)
    {
        headerClickedNode = null;
        bodyClickedNode = null;
        currentLineStartPoint = null;
    }

    private void HandleRightMouseDownEvent(Event e)
    {
        
    }

    private void HandleLeftMouseDownEvent(Event e)
    {
        selectedNodes.Clear();
        RogueGraphNode nodeAtMousePosition = GetNodeAtPosition(e.mousePosition);
        //if (nodeAtMousePosition != null)
        //{
        //    selectedNodes.Add(nodeAtMousePosition);
        //}
        //else
        //{
        //    selectedNodes.Clear();
        //}

        if (CanMoveNode(e.mousePosition))
        {
            headerClickedNode = nodeAtMousePosition;
            selectedNodes.Add(nodeAtMousePosition);
        }

        if (CanConnectNode(e.mousePosition))
        {
            bodyClickedNode = nodeAtMousePosition;
            currentLineStartPoint = nodeAtMousePosition.GetCenterOfBody();
        }
    }

    private void HandleRightMouseDragEvent(Event e)
    {
        
    }

    private void HandleLeftMouseDragEvent(Event e)
    {
        if (headerClickedNode == null)
        {
            dragOffest += e.delta * 0.5f;
        }
        else
        {
            //foreach (RogueGraphNode node in selectedNodes)
            //{
            //    node.Move(e.delta);
            //}
            headerClickedNode.Move(e.delta);
        }
    }

    private void ShowContextMenu(Vector2 mousePosition)
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

    private RogueGraphNode GetNodeAtPosition(Vector2 position)
    {
        foreach (RogueGraphNode node in currentGraph.nodes)
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
        foreach (RogueGraphNode node in currentGraph.nodes)
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
        foreach (RogueGraphNode node in currentGraph.nodes)
        {
            if (node.IsMouseInBody(mousePosition))
            {
                return true;
            }
        }
        return false;
    }
}
