using System;
using UnityEngine;

public class UIViewStateManager : MonoBehaviour
{
    public static UIViewStateManager Instance { get; private set; }

    public event EventHandler<UIBeingViewed> UpdateUiBeingViewedEvent;
    private SacrificeStoreRogueManager sacrificeStoreRogueManager;

    private static UIBeingViewed currentUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        currentUI = UIBeingViewed.Null;

        sacrificeStoreRogueManager = FindFirstObjectByType<SacrificeStoreRogueManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentUI == UIBeingViewed.LevelUp)
            return;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleUI(UIBeingViewed.Inventory);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleUI(UIBeingViewed.Construction);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleUI(UIBeingViewed.Craft);
        }
        else if (Input.GetKeyDown(KeyCode.R) && sacrificeStoreRogueManager.IsPlayerInConstructionRange())
        {
            ToggleUI(UIBeingViewed.Sacrifice);
        }
        else if (Input.GetKeyDown(KeyCode.Escape)) // Check for the Escape key
        {
            CollaspeAllUI(); // Call the method to collapse all UI
        }
    }

    private void ToggleUI(UIBeingViewed ui)
    {
        currentUI = currentUI == ui ? UIBeingViewed.Null : ui;
        UpdateUiBeingViewedEvent?.Invoke(this, currentUI);
    }

    public static bool IsViewingUI()
    {
        return currentUI != UIBeingViewed.Null;
    }

    public static bool IsViewingConstruction()
    {
        return currentUI == UIBeingViewed.Construction;
    }

    public static UIBeingViewed GetCurrentUI()
    {
        return currentUI;
    }

    public void CollaspeAllUI()
    {
        ToggleUI(UIBeingViewed.Null);
    }

    public void DisplayChestUI()
    {
        ToggleUI(UIBeingViewed.Chest);
    }

    public void ToggleLevelUpUI()
    {
        ToggleUI(UIBeingViewed.LevelUp);
    }

    public void ToggleSacrificeUI()
    {
        ToggleUI(UIBeingViewed.Sacrifice);
    }

    // Specific function to toggle CraftUI (CraftMenu and CraftUI)
    public void ToggleCraftUI()
    {
        // Toggle between the Craft Menu and Craft UI
        currentUI = currentUI == UIBeingViewed.Craft ? UIBeingViewed.Null : UIBeingViewed.Craft;
        UpdateUiBeingViewedEvent?.Invoke(this, currentUI);
    }
}

public enum UIBeingViewed
{
    Null,
    Construction,
    Inventory,
    LevelUp,
    Sacrifice,
    Craft,
    Chest
}