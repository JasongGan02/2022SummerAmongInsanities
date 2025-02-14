using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SacrificeStoreRogueManager : RogueManagerBase
{
    protected override string NameRogueUI => "SacrificeStoreUI";
    [SerializeField] private KeyCode openSacrificeKey = KeyCode.R;
    private CoreArchitectureController coreController;
    private int totalPurchases;
    private const float CoreControllerTimeout = 10f;
    private List<RogueGraphNode> storedNodes = new List<RogueGraphNode>();


    protected override void Start()
    {
        base.Start();
        StartCoroutine(InitializeCoreControllerCoroutine());
        
        if (insufficientFundsText != null)
        {
            insufficientFundsText.text = "";
        }
    }
    
    private IEnumerator InitializeCoreControllerCoroutine()
    {
        float elapsedTime = 0f;
        // Attempt to find the CoreArchitectureController every frame until timeout
        while (coreController == null && elapsedTime < CoreControllerTimeout)
        {
            coreController = FindObjectOfType<CoreArchitectureController>();
            if (coreController == null)
            {
                yield return null; // Wait for the next frame
                elapsedTime += Time.deltaTime;
            }
        }

        if (coreController == null)
        {
            Debug.LogError("CoreArchitectureController not found in the scene after waiting.");
            // Optionally, you can disable this manager or handle the missing controller gracefully
            this.enabled = false;
        }
        else
        {
            //Debug.Log("CoreArchitectureController successfully found.");
            // You can perform additional initialization here if needed
        }
    }
    
    public bool IsPlayerInConstructionRange()
    {
        return coreController != null && coreController.IsPlayerInConstructionRange();
    }
    
    protected override void OnUIUpdated(object sender, UIBeingViewed ui)
    {
        rogueUI.SetActive(ui == UIBeingViewed.Sacrifice);
        if (ui == UIBeingViewed.Sacrifice)
        {
            AddBuffs(false);
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    protected override void AddBuffs(bool needReroll)
    {
        ClearBuffCards();

        List<RogueGraphNode> nodes;
        if (storedNodes.Count > 0 && !needReroll)
        {
            nodes = new List<RogueGraphNode>(storedNodes);
        }
        else
        {
            nodes = GetRandomBuffNodes();
            storedNodes = new List<RogueGraphNode>(nodes);
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            RogueGraphNode node = nodes[i];
            GameObject buffCard = Instantiate(buffSelectionTemplate);
            Transform ashCostUI = buffCard.transform.Find("AshCostText");
            int ashCost = CalculateAshCost(node.quality);
            ashCostUI.GetComponent<TMP_Text>().text = $"{ashCost} 烬";
            BuffSelectionController buffSelectionController = buffCard.GetComponent<BuffSelectionController>();
            buffSelectionController.Init(node, buffContainer.transform, new Vector2(460 + 500 * i, 590f));
            buffSelectionController.onBuffSelectedEvent += HandleBuffSelectedEvent;
        }
    }
    
    private int CalculateAshCost(Quality quality)
    {
        float baseCost = CostCalculator.GetBaseCost("RogueManagerBase");
        return Mathf.RoundToInt(CostCalculator.CalculateFinalCost(baseCost, quality.cost));
    }
    
    protected override void HandleBuffSelectedEvent(object sender, RogueGraphNode node)
    {
        float baseCost = CostCalculator.CalculateBaseCostAndIncrement("RogueManagerBase");
        int nodeCost =  Mathf.RoundToInt(CostCalculator.CalculateFinalCost(baseCost, node.quality.cost));
        
        if (playerExperience == null)
        {
            playerExperience = FindObjectOfType<PlayerExperience>();
            if (playerExperience == null)
                return;
        }
        
        // Check if the player has enough EXP
        if (playerExperience.SpendAsh(nodeCost))
        {
            base.HandleBuffSelectedEvent(sender, node);
            UpdateRerollUI(nodeCost);
            totalPurchases++;
            
            // Auto-refill the store after a successful purchase
            StartCoroutine(RefillStoreCoroutine());
        }
        else
        {
            Debug.Log("Not enough ash to perform this sacrifice.");
            ShowInsufficientFundsNotification(GetPurchaseInsufficientFundsMessage());
        }
    }
    
    private IEnumerator RefillStoreCoroutine()
    {
        // Wait for a short moment in real-time to let any animations or effects finish
        yield return new WaitForSecondsRealtime(0.2f);

        // Generate and display new nodes
        storedNodes.Clear();
        AddBuffs(false);
    }


    
   
}
