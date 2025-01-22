using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RogueGraphNode : ScriptableObject
{
    public string blessingName = "Unnamed Node"; // 福赠名称
    public EffectObject effect;
    public bool isReselectable = true;
    public float baseWeight;
    public Quality quality;
    public Sprite blessingIcon;
    private RogueGraph containerGraph;
    private bool isNameManuallyChanged = false;
    public bool isRoot = false;


  

    [SerializeField]
    public List<RogueGraphNode> childNodes = new();
    [SerializeField]
    public List<RogueGraphNode> parentNodes = new();
    
    public float GetTotalWeight()
    {
        return baseWeight + quality.weight;
    }

#if UNITY_EDITOR
    [SerializeField]
    private Rect rect;
    private Rect headerRect;

    private string BlessingName // Property for 福赠名称
    {
        get => blessingName;
        set
        {
            if (blessingName != value)
            {
                blessingName = value;
                isNameManuallyChanged = true; // Mark name as manually changed
                UpdateAssetName();
            }
        }
    }

    private void UpdateAssetName()
    {
        // If Unity is refreshing, defer the update
        if (EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += UpdateAssetName;
            return;
        }

        // Perform the update safely
        if (!string.IsNullOrWhiteSpace(blessingName) && name != blessingName)
        {
            name = blessingName; // Update the asset's internal name
            EditorUtility.SetDirty(this); // Mark the object as dirty
            AssetDatabase.SaveAssets(); // Save the changes to the asset database
        }
    }


    
    private static readonly Dictionary<ItemRarity, (Color color, float weight, float cost)> RarityMappings = new()
    {
        { ItemRarity.Common, (Color.gray, 20f, 0f) },
        { ItemRarity.Uncommon, (Color.green, 10f, 0.3f) },
        { ItemRarity.Rare, (Color.blue, 3f, 0.8f) },
        { ItemRarity.Epic, (new Color(0.58f, 0, 0.83f), 4f, 1.5f) }, // Purple
        { ItemRarity.Legendary, (Color.yellow, 5f, 2.5f) }
    };

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
        GUIStyle headerStyle = new GUIStyle
        {
            normal = { textColor = Color.white },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        headerRect = rect;
        headerRect.height = 20;
        headerRect.y = rect.y - 14;

        GUILayout.BeginArea(headerRect);
        GUILayout.Label(name, headerStyle); // Display node name
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

        float remainingHeight = rect.height - EditorGUIUtility.singleLineHeight - 10f;

        // Display Blessing Name as a text field
        Rect blessingNameFieldRect = new Rect(5f, 5f, rect.width - 10f, EditorGUIUtility.singleLineHeight);
        BlessingName = EditorGUI.TextField(blessingNameFieldRect, "Blessing Name", BlessingName);

        // Display Effect field
        Rect objectFieldRect = new Rect(5f, blessingNameFieldRect.yMax + 5f, rect.width - 10f, EditorGUIUtility.singleLineHeight);
        effect = EditorGUI.ObjectField(objectFieldRect, "Effect", effect, typeof(EffectObject), false) as EffectObject;
        

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

        Handles.DrawBezier(start, end, start, end, Color.white, null, ConnectingLineWidth);

        GUI.changed = true;
    }

    public void DrawConnectionLine(RogueGraphNode childNode)
    {
        Vector2 start = rect.center;
        Vector2 end = childNode.rect.center;
        Handles.DrawBezier(start, end, start, end, Color.white, null, ConnectingLineWidth);

        Vector2 direction = end - start;
        Vector2 midPoint = (start + end) / 2;
        Vector2 arrowHead = midPoint + direction.normalized * ConnectingLineArrowSize;
        Vector2 arrowTail1 = midPoint + new Vector2(-direction.y, direction.x).normalized * ConnectingLineArrowSize;
        Vector2 arrowTail2 = midPoint - new Vector2(-direction.y, direction.x).normalized * ConnectingLineArrowSize;

        Handles.DrawBezier(arrowHead, arrowTail1, arrowHead, arrowTail1, Color.white, null, ConnectingLineWidth);
        Handles.DrawBezier(arrowHead, arrowTail2, arrowHead, arrowTail2, Color.white, null, ConnectingLineWidth);
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
    
    private void OnValidate()
    {
        if (EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return; // Prevent execution during Unity refresh or playmode changes
        }

        UpdateAssetName(); // Safe call to update the asset name
        
        quality ??= new Quality();

        if (RarityMappings.TryGetValue(quality.rarity, out var mapping))
        {
            // Only update if the current values differ to prevent unnecessary changes
            if (quality.color != mapping.color || Mathf.Abs(quality.weight - mapping.weight) > Mathf.Epsilon)
            {
                quality.color = mapping.color;
                quality.weight = mapping.weight;
                quality.cost = mapping.cost;
                EditorUtility.SetDirty(this);
            }
        }
        else
        {
            Debug.LogWarning($"No mapping defined for ItemRarity: {quality.rarity}");
        }
    }

    

    private const float ConnectingLineWidth = 3f;
    private const float ConnectingLineArrowSize = 6f;
#endif
}



