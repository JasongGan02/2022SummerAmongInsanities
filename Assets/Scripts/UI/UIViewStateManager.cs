using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIViewStateManager : MonoBehaviour
{
    public event EventHandler<UIBeingViewed> UpdateUiBeingViewedEvent;

    private static UIBeingViewed currentUI;
    // Start is called before the first frame update
    void Start()
    {
        currentUI = UIBeingViewed.Null;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentUI == UIBeingViewed.LevelUp)
            return; 
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleUI(UIBeingViewed.Inventory);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleUI(UIBeingViewed.Construction);
        }
        else if (Input.GetKeyDown(KeyCode.P)) 
        {
            ToggleUI(UIBeingViewed.Craft);
        }
        else if (Input.GetKeyDown(KeyCode.Escape)) // Check for the Escape key
        {
            collaspeAllUI(); // Call the method to collapse all UI
        }
    }

    private void ToggleUI(UIBeingViewed ui)
    {
        currentUI = currentUI == ui ? UIBeingViewed.Null : ui;
        UpdateUiBeingViewedEvent?.Invoke(this, currentUI);
    }

    public static bool isViewingUI()
    {
        return currentUI != UIBeingViewed.Null;
    }

    public static bool GetCurUI()
    {
        return currentUI==UIBeingViewed.Construction;
    }

    public void collaspeAllUI()
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

}

public enum UIBeingViewed {
    Null,
    Construction,
    Inventory,
    LevelUp,
    Sacrifice,
    Craft,
    Chest
}