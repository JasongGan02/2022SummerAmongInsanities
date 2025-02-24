using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class ConstructionModeManager : MonoBehaviour
{
    public static bool IsInConstructionMode { get; private set; } = false;

    [Header("References")]
    [SerializeField] private CameraMover cameraMover;

    [Header("UI Elements")]
    [SerializeField] private GameObject constructionUI;
    [SerializeField] private GameObject energyText;

    [Header("Energy Settings")]
    [SerializeField] private int maxEnergy = 100;
    [SerializeField] private int currentEnergy = 0;
    

    private CoreArchitectureController coreArchitecture;
    private UIViewStateManager uiViewStateManager;

    void Start()
    {
        // 优先从单例或者场景中找到对应组件
        if (coreArchitecture == null)
        {
            StartCoroutine(WaitForCoreArchitectureAndInitialize());
        }

        if (uiViewStateManager == null)
        {
            uiViewStateManager = UIViewStateManager.Instance;
        }

        if (cameraMover == null)
        {
            cameraMover = FindFirstObjectByType<CameraMover>();
        }

        UpdateEnergyText();

        // 订阅 UI 状态切换事件
        if (uiViewStateManager != null)
        {
            uiViewStateManager.UpdateUiBeingViewedEvent += OnUpdateUiBeingViewed;
        }
    }

    private void OnDestroy()
    {
        if (uiViewStateManager != null)
        {
            uiViewStateManager.UpdateUiBeingViewedEvent -= OnUpdateUiBeingViewed;
        }
    }

    /// <summary>
    /// 根据 UI 状态更新是否进入建造模式
    /// </summary>
    // ConstructionModeManager.cs
    private void OnUpdateUiBeingViewed(object sender, UIBeingViewed ui)
    {
        // 增强检查逻辑
        if (ShouldBlockModeSwitch(ui)) return;

        IsInConstructionMode = (ui == UIBeingViewed.Construction);
        ExecuteModeTransition();
    }

    private bool ShouldBlockModeSwitch(UIBeingViewed ui)
    {
        // 1. 检查过渡状态
        if (cameraMover != null && cameraMover.IsInTransition) return true;

        // 2. 检查是否已经是目标状态
        bool targetMode = (ui == UIBeingViewed.Construction);
        if (IsInConstructionMode == targetMode) return true;

        // 3. 检查必要组件
        return coreArchitecture == null || cameraMover == null;
    }

    private void ExecuteModeTransition()
    {
        if (IsInConstructionMode)
        {
            EnterConstructionMode();
        }
        else
        {
            ExitConstructionMode();
        }
    }

    /// <summary>
    /// 进入建造模式：暂停游戏、显示 UI、打开核心建筑的建造范围、摄像机聚焦核心建筑
    /// </summary>
    private void EnterConstructionMode()
    {
        if (coreArchitecture == null || (cameraMover != null && cameraMover.IsInTransition)) return;

        Time.timeScale = 0f;
        constructionUI.SetActive(true);
        coreArchitecture.ToggleConstructionIndicator(true);

        // 使用核心建筑位置作为摄像机焦点（调整 Z 坐标）
        Vector3 corePos = coreArchitecture.transform.position;
        cameraMover.FocusOnCore(new Vector3(corePos.x, corePos.y, -1));
    }

    /// <summary>
    /// 退出建造模式：恢复游戏、隐藏 UI、关闭建造范围、摄像机重置回玩家位置
    /// </summary>
    private void ExitConstructionMode()
    {
        if (coreArchitecture == null || (cameraMover != null && cameraMover.IsInTransition)) return;

        Time.timeScale = 1f;
        constructionUI.SetActive(false);
        coreArchitecture.ToggleConstructionIndicator(false);
        cameraMover.ResetToPlayer();
    }

    /// <summary>
    /// 更新能量显示文本
    /// </summary>
    private void UpdateEnergyText()
    {
        if (energyText != null)
        {
            TextMeshProUGUI energyTMP = energyText.GetComponent<TextMeshProUGUI>();
            if (energyTMP != null)
            {
                energyTMP.SetText("Energy: {0}/{1}", currentEnergy, maxEnergy);
            }
        }
    }

    /// <summary>
    /// 检查当前能量是否足够建造消耗
    /// </summary>
    public bool CheckEnergyAvailableForConstruction(int cost)
    {
        return (currentEnergy + cost) <= maxEnergy;
    }

    /// <summary>
    /// 消耗能量并更新显示
    /// </summary>
    public void ConsumeEnergy(int cost)
    {
        currentEnergy += cost;
        UpdateEnergyText();
    }

    /// <summary>
    /// 可供外部设置是否进入建造模式（如按键切换）
    /// </summary>
    public void SetConstructionMode(bool status)
    {
        IsInConstructionMode = status;
        if (status)
        {
            EnterConstructionMode();
        }
        else
        {
            ExitConstructionMode();
        }
    }

    IEnumerator WaitForCoreArchitectureAndInitialize()
    {
        while (CoreArchitectureController.Instance == null)
        {
            yield return null;
        }

        coreArchitecture = CoreArchitectureController.Instance;
    }
}