using UnityEngine;
using System.Collections;

public class ConstructionRangeIndicator : MonoBehaviour
{
    // 引用核心建筑的控制器（可以通过 CoreArchitectureController.Instance 获取）
    public CoreArchitectureController coreController;

    // 指定指示器的颜色（包含透明度），例如半透明绿色
    public Color indicatorColor = new Color(0f, 1f, 0f, 0.3f);

    // 指定指示器的固定高度（世界单位）
    public float indicatorHeight = 5f;

    // 纹理和 Sprite 的单位像素数（决定清晰度）
    public float pixelsPerUnit = 100f;

    // 内部使用的 SpriteRenderer 组件
    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// 在 Awake 中添加或获取 SpriteRenderer 组件
    /// </summary>
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.enabled = false;
    }

    /// <summary>
    /// 在 Start 中通过协程等待核心建筑实例不为 null，然后生成 Sprite 并设置位置
    /// </summary>
    void Start()
    {
        // 如果没有手动指定 coreController，则启动协程等待核心建筑实例
        if (coreController == null)
        {
            StartCoroutine(WaitForCoreController());
        }
        else
        {
            SetupIndicator();
        }
    }

    public void SetVisibility(bool visible)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = visible;
        }
    }

    /// <summary>
    /// 协程：等待 CoreArchitectureController.Instance 不为 null 后赋值，并设置指示器
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForCoreController()
    {
        while (CoreArchitectureController.Instance == null)
        {
            yield return null;
        }

        coreController = CoreArchitectureController.Instance;
        SetupIndicator();
    }

    /// <summary>
    /// 设置指示器：生成 Sprite 并将位置对齐到核心建筑
    /// </summary>
    void SetupIndicator()
    {
        // 根据核心的构造距离生成指示器 Sprite
        GenerateIndicatorSprite();
        // 将指示器的位置设置为核心建筑的位置（由于 Sprite 的 pivot 为 (0.5, 0)，
        // 所以 Sprite 的底部中心点正好与核心建筑的 position 对齐）
        transform.position = coreController.transform.position;
    }

    /// <summary>
    /// 根据核心的构造距离、固定高度和指定颜色生成矩形 Sprite
    /// </summary>
    public void GenerateIndicatorSprite()
    {
        // 获取构造距离（作为宽度）
        float indicatorWidth = coreController.GetConstructableDistance();

        // 根据世界单位和 pixelsPerUnit 计算纹理的像素尺寸
        int textureWidth = Mathf.RoundToInt(indicatorWidth * pixelsPerUnit);
        int textureHeight = Mathf.RoundToInt(indicatorHeight * pixelsPerUnit);

        // 创建一个新的 Texture2D
        Texture2D texture = new Texture2D(textureWidth, textureHeight);

        // 填充纹理所有像素为指定颜色
        Color[] pixelColors = new Color[textureWidth * textureHeight];
        for (int i = 0; i < pixelColors.Length; i++)
        {
            pixelColors[i] = indicatorColor;
        }

        texture.SetPixels(pixelColors);
        texture.Apply();

        // 创建 Sprite，设置 pivot 为 (0.5, 0) —— 底部中心
        Sprite indicatorSprite = Sprite.Create(texture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0));

        // 将生成的 Sprite 赋给 SpriteRenderer
        spriteRenderer.sprite = indicatorSprite;
    }

    /// <summary>
    /// 检查传入的玩家对象是否处于这个建造区域内
    /// 区域水平范围：[核心 x - 宽度/2, 核心 x + 宽度/2]，垂直范围：[核心 y, 核心 y + indicatorHeight]
    /// </summary>
    /// <param name="player">玩家的 GameObject</param>
    /// <returns>在区域内返回 true，否则返回 false</returns>
    public bool IsPlayerWithinConstructionArea(GameObject player)
    {
        if (player == null) return false;

        // 核心建筑的位置（指示器的位置即核心位置）
        Vector3 corePos = transform.position;

        // 宽度来自核心的构造距离
        float indicatorWidth = coreController.GetConstructableDistance();
        float halfWidth = indicatorWidth / 2f;

        // 获取玩家的位置
        Vector3 playerPos = player.transform.position;

        // 检查水平方向是否在范围内
        bool inXRange = playerPos.x >= (corePos.x - halfWidth) && playerPos.x <= (corePos.x + halfWidth);
        // 检查垂直方向是否在范围内（从核心的 y 到核心 y + 指示器高度）
        bool inYRange = playerPos.y >= corePos.y && playerPos.y <= (corePos.y + indicatorHeight);

        return inXRange && inYRange;
    }
}