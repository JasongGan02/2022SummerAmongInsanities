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
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleUI(UIBeingViewed.Inventory);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleUI(UIBeingViewed.Construction);
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
}

public enum UIBeingViewed {
    Null,
    Construction,
    Inventory
}