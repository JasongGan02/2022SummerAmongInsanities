using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreArchitectureController : CharacterController
{
    public static CoreArchitectureController Instance { get; private set; }

    private CoreStats coreStats => (CoreStats)currentStats;
    private GameObject player;
    private ConstructionRangeIndicator constructionRangeIndicator;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        constructionRangeIndicator = transform.Find("ConstructionRangeIndicator").gameObject.GetComponent<ConstructionRangeIndicator>();
        audioEmitter?.PlayClipFromCategory("GearRotating", false);
    }

    protected override void Update()
    {
        base.Update();
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
    }

    public void ToggleConstructionIndicator(bool setActive)
    {
        if (constructionRangeIndicator != null)
        {
            constructionRangeIndicator.gameObject.SetActive(setActive);
        }
        else
        {
            Debug.LogWarning("ConstructionRangeIndicator 未找到！");
        }
    }


    /// <summary>
    /// 判断玩家是否处于建筑范围内
    /// </summary>
    public bool IsPlayerInConstructionRange()
    {
        if (player == null) return false;
        return constructionRangeIndicator.IsPlayerWithinConstructionArea(player);
    }

    public float GetConstructableDistance()
    {
        return coreStats.constructableDistance;
    }

    protected override void Die()
    {
        DataPersistenceManager.instance?.GameOver();
        SceneManager.LoadSceneAsync("MainMenu");
    }
}