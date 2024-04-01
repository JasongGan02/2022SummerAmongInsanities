using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageDisplay : MonoBehaviour
{
    private Canvas canvas;
    private GameObject damageTextPrefab;

    void Awake()
    {
        // Attempt to find the Canvas in the scene (you might want to tag your canvas for more precise finding)
        canvas = FindObjectOfType<Canvas>();

        // Load the Damage Text Prefab from Resources folder
        damageTextPrefab = Resources.Load<GameObject>("DamageDisplay");
        if (damageTextPrefab == null)
        {
            Debug.LogError("Damage Text Prefab not found in Resources. Please check the path.");
        }
    }
        
    public void ShowDamage(float amount)
    {
        if (canvas == null || damageTextPrefab == null)
        {
            Debug.LogError("Required components not found. Please ensure a Canvas is present and the DamageTextPrefab is correctly set.");
            return;
        }

        GameObject textObj = Instantiate(damageTextPrefab, canvas.transform, false);
        Text damageText = textObj.GetComponent<Text>();
        if (damageText == null)
        {
            Debug.LogError("DamageTextPrefab does not contain a Text component.");
            return;
        }
        int roundedAmount = Mathf.RoundToInt(amount);
        // Set the text to the rounded integer amount
        damageText.text = roundedAmount.ToString();

        // Assume canvas is the Canvas component, and we need its RectTransform
        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();

        // Calculate the position to place the damage text
        Vector2 worldPosition = transform.position + new Vector3(0, 2f, 0); // Adjust the Y offset as needed
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        // Convert screen position to canvas local position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition, canvas.GetComponent<Canvas>().worldCamera, out Vector2 localPoint);
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(localPoint.x, localPoint.y, 0);

        // Start the animation coroutine
        StartCoroutine(AnimateDamageText(rectTransform));
    }


    private IEnumerator AnimateDamageText(RectTransform textTransform)
    {
        float duration = 1f; // Duration of the animation
        Vector3 startpos = textTransform.anchoredPosition;
        Vector3 endpos = startpos + new Vector3(0, 100f, 0); // Adjust as needed for the "jump" effect

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            textTransform.anchoredPosition = Vector3.Lerp(startpos, endpos, t / duration);
            yield return null;
        }

        Destroy(textTransform.gameObject);
    }
}
