using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class RogueGraphEditorWindow : EditorWindow
{
#if UNITY_EDITOR  
    private static RogueGraphEditorWindowController controller;

    private GUIStyle normalStyle;
    private GUIStyle selectedStyle;

    [OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int instanceId, int line)
    {
        RogueGraph graph = EditorUtility.InstanceIDToObject(instanceId) as RogueGraph;

        if (graph != null)
        {
            GetWindow<RogueGraphEditorWindow>();
            controller = new RogueGraphEditorWindowController(graph);

            controller.SetupWindow();

            return true;
        }

        return false;
    }

    private void OnEnable()
    {
        normalStyle = new GUIStyle();
        normalStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        normalStyle.normal.textColor = Color.white;
        normalStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        normalStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        selectedStyle = new GUIStyle();
        selectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        selectedStyle.normal.textColor = Color.white;
        selectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        selectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
    }

    private void OnGUI()
    {
        controller.ProcessEvent(Event.current);
        DrawCurrentLine();
        DrawNodes();
        
    }

    private void DrawNodes()
    {
        foreach (RogueGraphNode node in controller.currentGraph.nodes)
        {
            if (controller.selectedNodes.Contains(node))
            {
                node.Draw(selectedStyle);
            }
            else
            {
                node.Draw(normalStyle);
            }
        }
        Repaint();
    }

    private void DrawCurrentLine()
    {
        if (controller.currentLineStartPoint != null)
        {
            Vector2 currentLineStartPoint = (Vector2) controller.currentLineStartPoint;
            Vector2 mousePosition = Event.current.mousePosition;
            Handles.DrawBezier(currentLineStartPoint, mousePosition, currentLineStartPoint, mousePosition, Color.white, null, connectingLineWidth);
            Repaint();
        }
    }

    private const int nodePadding = 25;
    private const int nodeBorder = 12;
    private const float connectingLineWidth = 3f;
#endif
}   
