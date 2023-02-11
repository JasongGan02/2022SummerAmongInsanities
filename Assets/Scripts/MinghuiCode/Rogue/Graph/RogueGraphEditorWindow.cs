using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class RogueGraphEditorWindow : EditorWindow
{
#if UNITY_EDITOR  
    private static RogueGraphEditorWindowController controller;

    [OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int instanceId, int line)
    {
        RogueGraph graph = EditorUtility.InstanceIDToObject(instanceId) as RogueGraph;

        if (graph != null)
        {
            GetWindow<RogueGraphEditorWindow>();
            controller = new RogueGraphEditorWindowController(graph);

            controller.CreateRootNode();

            return true;
        }

        return false;
    }

    private void OnGUI()
    {
        controller.ProcessEvent(Event.current);
        controller.parentNode?.DrawConnectionLineToMousePosition(Event.current.mousePosition);
        controller.graph?.DrawConnectionLines();
        controller.graph?.DrawNodes();
        Repaint();
    }

#endif
}   
