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
            Debug.Log("CoreArchitectureController successfully found.");
            // You can perform additional initialization here if needed
        }
    }
    
    protected void Update() 
    {
        if (Input.GetKeyDown(openSacrificeKey) && coreController != null && coreController.IsPlayerInControlRange())
        {
            OnPlayerOpenSacrificeStore();
        }
    }
    
    protected override void OnUIUpdated(object sender, UIBeingViewed ui)
    {
        rogueUI.SetActive(ui == UIBeingViewed.Sacrifice);
    }

    private void OnPlayerOpenSacrificeStore()
    {
        uiViewStateManager.ToggleSacrificeUI();
        AddBuffs();
    }

    protected override void AddBuffs()
    {
        ClearBuffCards();
        List<RogueGraphNode> nodes = GetRandomBuffNodes();
        for (int i = 0; i < nodes.Count; i++)
        {
            RogueGraphNode node = nodes[i];
            GameObject buffCard = Instantiate(buffSelectionTemplate);
            Transform ashCostUI = buffCard.transform.Find("AshCostText");
            int ashCost = CalculateAshCost(node.quality);
            ashCostUI.GetComponent<TMP_Text>().text = $"{ashCost} 烬";
            BuffSelectionController buffSelectionController = buffCard.GetComponent<BuffSelectionController>();
            buffSelectionController.Init(node, buffContainer.transform, new Vector2(460 + 500 * i, 590f));
            buffSelectionController.OnBuffSelectedEvent += HandleBuffSelectedEvent;
            buffSelectionController.OnBuffHoverEnterEvent += ShowHoveringBuffUI;
            buffSelectionController.OnBuffHoverExitEvent += HideHoveringBuffUI;
        }
    }

    private const float INIT_COST = 10f;
    private const float GROWTH_RATE = 0.045f;
    private const float NONLINEAR_FACTOR = 2f;

    private int CalculateAshCost(Quality quality)
    {
        // Pfinal = Pbase * (1 + Pquality)
        // Pbase = Pinit * (1 + a * totalPurchase^b)
    
        float basePrice = INIT_COST * (1 + GROWTH_RATE * Mathf.Pow(totalPurchases, NONLINEAR_FACTOR));
        float finalPrice = basePrice * (1 + quality.cost);
    
        return Mathf.RoundToInt(finalPrice);
    }
    
    protected override void HandleBuffSelectedEvent(object sender, RogueGraphNode node)
    {
        int nodeCost = CalculateAshCost(node.quality);
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
        // Wait for a short moment to let any animations or effects finish
        yield return new WaitForSeconds(0.2f);
        
        // Refill the store with new items
        AddBuffs();
    }
    
   
}
