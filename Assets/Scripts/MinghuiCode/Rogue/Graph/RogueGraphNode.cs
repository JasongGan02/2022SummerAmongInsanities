using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RogueGraphNode : ScriptableObject
{
    public Buff buff = new();
    private RogueGraph containerGraph;

#if UNITY_EDITOR
    private Rect rect;
    private Rect headerRect;

    public void Init(Rect rect, RogueGraph containerGraph)
    {
        this.rect = rect;
        
        this.containerGraph = containerGraph;
        EditorUtility.SetDirty(this);
    }

    public void Draw(GUIStyle style)
    {
        GUIStyle test = new GUIStyle();
        
        headerRect = rect;
        headerRect.height = 20;
        headerRect.y = rect.y - 20;
        GUILayout.BeginArea(headerRect, test);
        GUILayout.Button("header");
        GUILayout.EndArea();

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
#endif
}
