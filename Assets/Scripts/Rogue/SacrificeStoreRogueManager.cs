using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SacrificeStoreRogueManager : RogueManagerBase
{
    [SerializeField] private KeyCode openSacrificeKey = KeyCode.R;
    [SerializeField] private TMP_Text insufficientFundsText;
    private CoreArchitectureController coreController;

    protected override void Start()
    {
        base.Start();
        coreController = FindObjectOfType<CoreArchitectureController>();
        if (coreController == null)
        {
            Debug.LogError("CoreArchitectureController not found in the scene.");
        }

        if (insufficientFundsText != null)
        {
            insufficientFundsText.text = "";
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
    }

    /*protected override void AddBuffs()
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
    */

    /*private int CalculateAshCost(float baseCost)
    {
        
    }
    
    protected override void HandleBuffSelectedEvent(object sender, RogueGraphNode node)
    {
        int nodeCost = node.quality.cost;
        if (inventory.HasEnoughEXP(nodeCost))
        {
            inventory.SpendExp(nodeCost);
            base.HandleBuffSelectedEvent(sender, node);
            UpdateRerollUI(nodeCost);
        }
        else
        {
            Debug.Log("Not enough EXP to perform this sacrifice.");
            ShowInsufficientFundsNotification();
        }
    }*/

    private void ShowInsufficientFundsNotification()
    {
        if (insufficientFundsText != null)
        {
            insufficientFundsText.text = "Not enough EXP to perform this sacrifice.";
            StartCoroutine(HideNotificationAfterTime(2f));
        }
    }

    private IEnumerator HideNotificationAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        if (insufficientFundsText != null)
        {
            insufficientFundsText.text = "";
        }
    }
}
