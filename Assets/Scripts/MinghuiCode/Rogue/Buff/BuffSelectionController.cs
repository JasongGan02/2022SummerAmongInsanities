using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class BuffSelectionController : MonoBehaviour, IPointerClickHandler
{
    private RogueGraphNode node;
    public EventHandler<RogueGraphNode> OnBuffSelectedEvent;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnBuffSelectedEvent?.Invoke(this, node);
    }

    public void Init(RogueGraphNode node, Transform parent, Vector2 position)
    {
        this.node = node;
        TMP_Text buffName = transform.Find(NAME_BUFF_NAME_TEXT).GetComponent<TMP_Text>();
        buffName.text = node.buff.name;
        transform.SetParent(parent);
        transform.position = position;
    }

    private const string NAME_BUFF_NAME_TEXT = "BuffNameText";
}