using UnityEngine;
using UnityEngine.UI;

public class ConstructionProgress : MonoBehaviour
{
    private float constructionTime;
    private float elapsedTime = 0f;
    private Image progressBar;

    /// <summary>
    /// Initialize the progress component with the construction duration.
    /// </summary>
    public void Initialize(float constructionTime)
    {
        this.constructionTime = constructionTime;
        CreateProgressBar();
    }

    /// <summary>
    /// Create a UI progress bar as a child of the tower GameObject.
    /// You can replace this with your own UI instantiation logic.
    /// </summary>
    private void CreateProgressBar()
    {
        GameObject canvasGO = new GameObject("ConstructionCanvas");
        canvasGO.transform.SetParent(transform);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject progressGO = new GameObject("ProgressBar");
        progressGO.transform.SetParent(canvasGO.transform);
        progressBar = progressGO.AddComponent<Image>();
        progressBar.color = Color.green;
        RectTransform rt = progressGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 10);
        rt.anchoredPosition = new Vector2(0, 50);
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedTime / constructionTime);
        if (progressBar != null)
        {
            progressBar.fillAmount = progress;
        }

        if (progress >= 1f)
        {
            // Construction complete: remove the progress bar and disable this component.
            if (progressBar != null)
            {
                Destroy(progressBar.gameObject);
            }

            Destroy(this);
        }
    }
}