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
    public EventHandler<RogueGraphNode> OnBuffHoverEnterEvent;
    public EventHandler<RogueGraphNode> OnBuffHoverExitEvent;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnBuffSelectedEvent?.Invoke(this, node);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Create and display the buff description UI
        Debug.Log("Hovering over buff card");
        OnBuffHoverEnterEvent?.Invoke(this, node);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Destroy the buff description UI when the pointer exits the buff card
        OnBuffHoverExitEvent?.Invoke(this, node);
    }
    public void Init(RogueGraphNode node, Transform parent, Vector2 position)
    {
        this.node = node;
        TMP_Text buffName = transform.Find(NAME_BUFF_NAME_TEXT).GetComponent<TMP_Text>();
        //buffName.text = node.buff.name;
        buffName.text = node.effect?.name ?? "No Effect Selected";
        
        transform.SetParent(parent);
        transform.position = position;
    }

    private const string NAME_BUFF_NAME_TEXT = "BuffNameText";
}
