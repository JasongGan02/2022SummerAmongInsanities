using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public abstract class RogueManagerBase : MonoBehaviour
{
    [SerializeField]
    protected RogueGraph graph;

    [SerializeField]
    protected GameObject buffSelectionTemplate;
    [SerializeField]
    protected GameObject hoveringBuffUIPrefab;

    protected Text insufficientFundsText;
    protected Button rerollButton;
    protected UIViewStateManager uiViewStateManager;
    protected GameObject rogueUI;
    protected TMP_Text selectedBuffText;
    protected GameObject buffContainer;
    protected AudioEmitter _audioEmitter;

    protected List<RogueGraphNode> selectedNodes = new List<RogueGraphNode>();
    
    
    //For reroll function
    protected Inventory inventory;
    [SerializeField]
    private int baseRerollCost = 5; // Base cost for the first reroll

    private int rerollCount = 0; // Tracks consecutive rerolls within the current day-night cycle


    protected virtual void Start()
    {
        uiViewStateManager = FindObjectOfType<UIViewStateManager>();
        _audioEmitter = GetComponent<AudioEmitter>();
        selectedNodes.Add(graph.rootNode);
        inventory = FindObjectOfType<Inventory>();
        SetupUI();
        
        uiViewStateManager.UpdateUiBeingViewedEvent += OnUIUpdated;
        GameEvents.current.OnDayStarted += ResetRerollCount;
    }
    
    protected virtual void OnDestroy()
    {
        if (uiViewStateManager != null)
        {
            uiViewStateManager.UpdateUiBeingViewedEvent -= OnUIUpdated;
        }
        GameEvents.current.OnDayStarted -= ResetRerollCount;
    }

    protected virtual void SetupUI()
    {
        rogueUI = GameObject.Find(NameRogueUI);
        insufficientFundsText = rogueUI.transform.Find(NAME_INSUFFICIENCY_TEXT).GetComponent<Text>();
        selectedBuffText = rogueUI.transform.Find(NAME_SELECTED_BUFF_TEXT).GetComponent<TMP_Text>();
        buffContainer = rogueUI.transform.Find(NAME_BUFF_CONTAINER).gameObject;
        rerollButton = rogueUI.transform.Find(NAME_REROLL_BUTTON).GetComponent<Button>();
        rerollButton.onClick.AddListener(OnRerollButtonClicked);
        rogueUI.SetActive(false);
    }

    protected abstract void OnUIUpdated(object sender, UIBeingViewed ui);
    
    protected virtual void AddBuffs()
    {
        ClearBuffCards();
        List<RogueGraphNode> nodes = GetRandomBuffNodes();
        for (int i = 0; i < nodes.Count; i++)
        {
            RogueGraphNode node = nodes[i];
            GameObject buffCard = Instantiate(buffSelectionTemplate);
            BuffSelectionController buffSelectionController = buffCard.GetComponent<BuffSelectionController>();
            buffSelectionController.Init(node, buffContainer.transform, new Vector2(460 + 500 * i, 590f));
            buffSelectionController.OnBuffSelectedEvent += HandleBuffSelectedEvent;
            buffSelectionController.OnBuffHoverEnterEvent += ShowHoveringBuffUI;
            buffSelectionController.OnBuffHoverExitEvent += HideHoveringBuffUI;
        }
    }
    
    protected virtual void ClearBuffCards()
    {
        // Remove existing buff cards
        foreach (Transform child in buffContainer.transform)
        {
            BuffSelectionController buffSelectionController = child.GetComponent<BuffSelectionController>();
            if (buffSelectionController != null)
            {
                buffSelectionController.OnBuffSelectedEvent -= HandleBuffSelectedEvent;
                buffSelectionController.OnBuffHoverEnterEvent -= ShowHoveringBuffUI;
                buffSelectionController.OnBuffHoverExitEvent -= HideHoveringBuffUI;
            }
            Destroy(child.gameObject);
        }
    }

    protected virtual List<RogueGraphNode> GetRandomBuffNodes()
    {
        List<RogueGraphNode> candidateNodes = new List<RogueGraphNode>();

        foreach (RogueGraphNode selectedNode in selectedNodes)
        {
            foreach (RogueGraphNode node in selectedNode.childNodes)
            {
                if (CanAddNodeToCandidates(node, candidateNodes))
                {
                    candidateNodes.Add(node);
                }
            }
        }

        if (candidateNodes.Count <= 3)
        {
            return candidateNodes;
        }
        else
        {
            return WeightedRandomSelection(candidateNodes, 3);
        }
    }
    
    protected virtual List<RogueGraphNode> WeightedRandomSelection(List<RogueGraphNode> candidateNodes, int selectionCount)
    {
        List<RogueGraphNode> selected = new List<RogueGraphNode>();
        List<RogueGraphNode> available = new List<RogueGraphNode>(candidateNodes);

        float totalWeight = 0f;
        foreach (var node in available)
        {
            totalWeight += node.quality.weight;
        }
        
        for (int i = 0; i < selectionCount && available.Count > 0; i++)
        {

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;
            RogueGraphNode selectedNode = null;

            foreach (var node in available)
            {
                cumulative += node.GetTotalWeight();
                if (randomValue <= cumulative)
                {
                    selectedNode = node;
                    break;
                }
            }

            if (selectedNode != null)
            {
                selected.Add(selectedNode);
                available.Remove(selectedNode);
                totalWeight -= selectedNode.quality.weight;
            }
        }

        return selected;
    }

    protected virtual bool CanAddNodeToCandidates(RogueGraphNode node, List<RogueGraphNode> candidateNodes)
    {
        if (selectedNodes.Contains(node) && !(node?.isReselectable ?? false)) return false;
        if (candidateNodes.Contains(node)) return false;
        if (node.parentNodes.Count <= 1) return true;

        foreach (RogueGraphNode parentNode in node.parentNodes)
        {
            if (!selectedNodes.Contains(parentNode)) return false;
        }

        return true;
    }

    protected virtual void HandleBuffSelectedEvent(object sender, RogueGraphNode node)
    {
        selectedNodes.Add(node);
        selectedBuffText.text += "\n" + (node.effect?.name ?? "No Effect Selected");
        _audioEmitter.PlayClipFromCategory("PlayerSelecting");

        // Apply the effect
        if (node.effect != null)
            node.effect.ExecuteEffectOnAType();
        
        ClearBuffCards();
    }
    

    protected virtual void ShowHoveringBuffUI(object sender, BuffSelectionController.OnBuffEventArgs args)
    {
        Vector2 mousePosition = Input.mousePosition + new Vector3(0, -100f);
        GameObject hoveringBuffUI = Instantiate(hoveringBuffUIPrefab, args.buffSelectionTemplate.transform.position + new Vector3(0, -300f), Quaternion.identity);
        hoveringBuffUI.transform.SetParent(args.buffSelectionTemplate.transform);

        TMP_Text descriptionText = hoveringBuffUI.GetComponentInChildren<TMP_Text>();
        descriptionText.text = args.node.effect?.description ?? "No Description Available";
    }

    protected virtual void HideHoveringBuffUI(object sender, BuffSelectionController.OnBuffEventArgs args)
    {
        Destroy(GameObject.FindGameObjectWithTag(NAME_HOVERING_BUFF));
    }

    private void OnRerollButtonClicked()
    {
        int currentCost = CalculateRerollCost();

        // Check if the player has enough EXP
        if (inventory.SpendExp(currentCost))
        {
            // Increment the reroll count for the next consecutive reroll
            rerollCount++;

            // Play reroll sound effect
            _audioEmitter.PlayClipFromCategory("PlayerReroll");
            
            // Add new buffs
            AddBuffs();

            // Update reroll cost UI
            UpdateRerollUI(currentCost);
        }
        else
        {
            // Inform the player that they don't have enough EXP
            Debug.Log("Not enough EXP to reroll.");
            // You can also trigger a UI notification here
            ShowInsufficientFundsNotification(GetRerollInsufficientFundsMessage());
        }
    }

    protected void ResetRerollCount()
    {
        rerollCount = 0;
    }
    
    private int CalculateRerollCost()
    {
        // Exponential increase: cost = baseCost * 2^rerollCount
        return baseRerollCost * (int)Mathf.Pow(1.132f, rerollCount);
    }
    
    protected void UpdateRerollUI(int lastCost)
    {
        // Example: Update a TMP_Text element to show the next reroll cost
        TMP_Text rerollCostText = rerollButton.transform.Find("RerollCostText").GetComponent<TMP_Text>();
        if (rerollCostText != null)
        {
            int nextCost = CalculateRerollCost();
            rerollCostText.text = $"Next Reroll Cost: {nextCost} EXP";
        }
    }

    protected virtual string GetRerollInsufficientFundsMessage()
    {
        return "您沒有足夠的燼獻祭以重新抽取賜福";
    }
    
    protected virtual string GetPurchaseInsufficientFundsMessage()
    {
        return "您沒有足夠的燼獻祭以獲得該賜福";
    }
    
    protected void ShowInsufficientFundsNotification(string message)
    {
        if (insufficientFundsText != null)
        {
            insufficientFundsText.text = message;
            StartCoroutine(HideNotificationAfterTime(2f));
        }
    }

    protected IEnumerator HideNotificationAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        if (insufficientFundsText != null)
        {
            insufficientFundsText.text = "";
        }
    }
    
    protected abstract string NameRogueUI { get; }
    protected const string NAME_INSUFFICIENCY_TEXT = "InsuffiencyText";
    protected const string NAME_SELECTED_BUFF_TEXT = "SelectedBuffText";
    protected const string NAME_BUFF_CONTAINER = "BuffContainer";
    protected const string NAME_HOVERING_BUFF = "HoveringBuffUI";
    protected const string NAME_REROLL_BUTTON = "RerollButton";
}
