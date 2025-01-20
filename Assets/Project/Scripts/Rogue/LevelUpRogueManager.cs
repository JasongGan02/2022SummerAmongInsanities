using System.Collections.Generic;
using UnityEngine;

public class LevelUpRogueManager : RogueManagerBase
{
    protected override string NameRogueUI => "LevelUpUI";
    private Queue<int> levelUpQueue = new Queue<int>();
    private bool isProcessingLevelUp = false;

    protected override void Start()
    {
        base.Start();
        if (GameEvents.current != null)
        {
            GameEvents.current.OnPlayerLevelUp += OnPlayerLevelUp;
        }
        else
        {
            Debug.LogError("GameEvents instance not found in the scene.");
        }

        
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (GameEvents.current != null)
        {
            GameEvents.current.OnPlayerLevelUp -= OnPlayerLevelUp;
        }
    }

    protected override void OnUIUpdated(object sender, UIBeingViewed ui)
    {
        rogueUI.SetActive(ui == UIBeingViewed.LevelUp);
    }

    private void OnPlayerLevelUp()
    {
        levelUpQueue.Enqueue(1);
        ProcessNextLevelUp();
    }

    private void ProcessNextLevelUp()
    {
        if (isProcessingLevelUp) return;
        if (levelUpQueue.Count == 0) return;

        isProcessingLevelUp = true;
        levelUpQueue.Dequeue();

        uiViewStateManager.ToggleLevelUpUI();
        PauseGame();

        AddBuffs(false);
    }

    protected override void HandleBuffSelectedEvent(object sender, RogueGraphNode node)
    {
        base.HandleBuffSelectedEvent(sender, node);

        isProcessingLevelUp = false;
        ResumeGame();
        uiViewStateManager.ToggleLevelUpUI();
        ProcessNextLevelUp();
    }

}