using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

public class DamageDisplay : MonoBehaviour
{
    private Canvas canvas;
    private GameObject damageTextPrefab;

    void Awake()
    {

    }

    public void ShowDamage(float amount,Transform enemyTransform)
    {
        var damageTextMeshPrefab = Resources.Load<TextMeshPro>("DamageDisplay");

        if (damageTextMeshPrefab == null)
        {
            Debug.LogError("Damage Text Mesh Prefab not found in Resources. Please check the path.");
            return;
        }

        // Instantiate the text mesh as a child of the enemy's transform
        TextMeshPro textMesh = Instantiate(damageTextMeshPrefab, enemyTransform.position, Quaternion.identity, enemyTransform);

        // Set the text to the damage amount
        textMesh.text = Mathf.RoundToInt(amount).ToString();

        // You may want to adjust the local position to offset the text from the enemy's center
        textMesh.transform.localPosition = new Vector3(0, 0.5f, 0);

        // Start the animation coroutine on the TextMeshPro component
        StartCoroutine(AnimateDamageText(textMesh));
    }
    private IEnumerator AnimateDamageText(TextMeshPro textMesh)
    {
        float duration = 1f; // Duration of the animation
        Vector3 startPos = textMesh.transform.localPosition;
        Vector3 endPos = startPos + new Vector3(0, 1f, 0); // Move text up over time

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            textMesh.transform.localPosition = Vector3.Lerp(startPos, endPos, t / duration);
            yield return null;
        }

        Destroy(textMesh.gameObject);
    }
}


