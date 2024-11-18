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
            int ashCost = CalculateAshCost(node.quality.cost);
            ashCostUI.GetComponent<TMP_Text>().text = $"{ashCost} EXP";
            BuffSelectionController buffSelectionController = buffCard.GetComponent<BuffSelectionController>();
            buffSelectionController.Init(node, buffContainer.transform, new Vector2(460 + 500 * i, 590f));
            buffSelectionController.OnBuffSelectedEvent += HandleBuffSelectedEvent;
            buffSelectionController.OnBuffHoverEnterEvent += ShowHoveringBuffUI;
            buffSelectionController.OnBuffHoverExitEvent += HideHoveringBuffUI;
        }
    }

    private int CalculateAshCost(float baseCost)
    {
        return (int) baseCost + 10;
    }
    
    protected override void HandleBuffSelectedEvent(object sender, RogueGraphNode node)
    {
        int nodeCost = CalculateAshCost(node.quality.cost);
        if (inventory.SpendExp(nodeCost))
        {
            inventory.SpendExp(nodeCost);
            base.HandleBuffSelectedEvent(sender, node);
            UpdateRerollUI(nodeCost);
        }
        else
        {
            Debug.Log("Not enough EXP to perform this sacrifice.");
            ShowInsufficientFundsNotification(GetPurchaseInsufficientFundsMessage());
        }
    }
    
   
}
