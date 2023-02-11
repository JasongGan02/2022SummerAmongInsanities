using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RogueGraphNode : ScriptableObject
{
    public Buff buff = new();
    private RogueGraph containerGraph;
    public bool isRoot = false;

    [SerializeField]
    public List<RogueGraphNode> childNodes = new();
    [SerializeField]
    public List<RogueGraphNode> parentNodes = new();

#if UNITY_EDITOR
    [SerializeField]
    private Rect rect;
    private Rect headerRect;

    public void Init(Rect rect, RogueGraph containerGraph, bool isRoot)
    {
        this.rect = rect;
        this.containerGraph = containerGraph;
        this.isRoot = isRoot;
        EditorUtility.SetDirty(this);
    }

    public void Draw(GUIStyle style)
    {
        DrawHeader();

        if (isRoot)
        {
            DrawRootNode(style);
        }
        else
        {
            DrawNormalNode(style);
        }
    }

    private void DrawHeader()
    {
        GUIStyle heander = new GUIStyle();

        headerRect = rect;
        headerRect.height = 20;
        headerRect.y = rect.y - 14;
        GUILayout.BeginArea(headerRect, heander);
        GUILayout.Button("");
        GUILayout.EndArea();
    }

    private void DrawRootNode(GUIStyle style)
    {
        GUILayout.BeginArea(rect, style);
        EditorGUILayout.LabelField("root");
        GUILayout.EndArea();
    }

    private void DrawNormalNode(GUIStyle style)
    {
        GUILayout.BeginArea(rect, style);
        EditorGUI.BeginChangeCheck();
        buff.name = EditorGUILayout.TextField(buff.name);
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }
        GUILayout.EndArea();
    }

    public void Move(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
       
    }

    public bool IsMouseIn(Vector2 mousePosition)
    {
        return rect.Contains(mousePosition) || headerRect.Contains(mousePosition);
    }

    public bool IsMouseInBody(Vector2 mousePosition)
    {
        return rect.Contains(mousePosition);
    }

    public bool IsMouseInHeader(Vector2 mousePosition)
    {
        return headerRect.Contains(mousePosition);
    }

    public Vector2 GetCenterOfBody()
    {
        return rect.center;
    }

    public void DrawConnectionLineToMousePosition(Vector2 mousePosition)
    {
        DrawConnectionLine(mousePosition);
    }

    private void DrawConnectionLine(Vector2 end)
    {
        Vector2 start = rect.center;

        Handles.DrawBezier(start, end, start, end, Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    public void DrawConnectionLine(RogueGraphNode childNode)
    {
        Vector2 start = rect.center;
        Vector2 end = childNode.rect.center;
        Handles.DrawBezier(start, end, start, end, Color.white, null, connectingLineWidth);

        Vector2 direction = end - start;
        Vector2 midPoint = (start + end) / 2;
        Vector2 arrowHead = midPoint + direction.normalized * connectingLineArrowSize;
        Vector2 arrowTail1 = midPoint + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTail2 = midPoint - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

        Handles.DrawBezier(arrowHead, arrowTail1, arrowHead, arrowTail1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHead, arrowTail2, arrowHead, arrowTail2, Color.white, null, connectingLineWidth);
    }

    public bool HasAncester(RogueGraphNode node)
    {
        Queue<RogueGraphNode> queue = new();
        queue.Enqueue(this);
        while(queue.Count > 0)
        {
            RogueGraphNode currentNode = queue.Dequeue();
            foreach(RogueGraphNode parentNode in currentNode.parentNodes)
            {
                queue.Enqueue(parentNode);
                if (parentNode == node)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 6f;
#endif
}
