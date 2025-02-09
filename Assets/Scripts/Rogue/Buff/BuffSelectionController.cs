using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BuffSelectionController : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler
{
    private RogueGraphNode node;
    public EventHandler<RogueGraphNode> onBuffSelectedEvent;
    public EventHandler<OnBuffEventArgs> onBuffHoverEnterEvent;
    public EventHandler<OnBuffEventArgs> onBuffHoverExitEvent;

    public void OnPointerClick(PointerEventData eventData)
    {
        onBuffSelectedEvent?.Invoke(this, node);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Create and display the buff description UI
        onBuffHoverEnterEvent?.Invoke(this, new OnBuffEventArgs(node, this.gameObject));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Destroy the buff description UI when the pointer exits the buff card
        onBuffHoverExitEvent?.Invoke(this, new OnBuffEventArgs(node, this.gameObject));
    }

    public void Init(RogueGraphNode node, Transform parent, Vector2 position)
    {
        this.node = node;
        TMP_Text buffName = transform.Find(NAME_BUFF_NAME_TEXT).GetComponent<TMP_Text>();
        //buffName.text = node.buff.name;
        buffName.text = node.name;
        TMP_Text descriptionText = transform.Find(DESCRIPTION).GetComponent<TMP_Text>();
        descriptionText.text = node.effect?.description ?? "No Description Available";
        Image image = transform.Find(ICON).GetComponent<Image>();
        if (node.effect != null) image.sprite = node.effect.icon;

        transform.SetParent(parent);
        transform.position = position;
    }

    public class OnBuffEventArgs : EventArgs
    {
        public RogueGraphNode node;
        public GameObject buffSelectionTemplate;

        public OnBuffEventArgs(RogueGraphNode node, GameObject buffSelectionTemplate)
        {
            this.node = node;
            this.buffSelectionTemplate = buffSelectionTemplate;
        }
    }

    private const string NAME_BUFF_NAME_TEXT = "BuffNameText";
    private const string DESCRIPTION = "Description";
    private const string ICON = "Icon";
}