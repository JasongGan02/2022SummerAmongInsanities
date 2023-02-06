using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RogueManager : MonoBehaviour
{
    [SerializeField]
    private RogueGraph graph;
    // Start is called before the first frame update
    void Start()
    {
        RogueGraphNode rootNode = graph.rootNode;
        foreach (RogueGraphNode node in rootNode.childNodes)
        {
            Debug.Log(node.buff.name);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
