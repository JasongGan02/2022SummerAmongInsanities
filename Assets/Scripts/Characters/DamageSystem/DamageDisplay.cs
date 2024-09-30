using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using static UnityEditorInternal.ReorderableList;

public class DamageDisplay : MonoBehaviour
{
    private float floatUpDistance = 1f;
    private float duration = 0.5f;

    private Color RedColor = Color.red;
    private Color BlueColor = Color.blue;
    private Color GreenColor = Color.green;
    private Color DefaultColor = Color.white;


    private GameObject DamageTextContainer;

    public void ShowDamage(float amount,Transform enemyTransform)
    {
        DamageTextContainer = GameObject.Find("DamageTextContainer");
        var damageTextMeshPrefab = Resources.Load<TextMeshPro>("DamageDisplay");

        if (damageTextMeshPrefab == null)
        {
            Debug.LogError("Damage Text Mesh Prefab not found in Resources. Please check the path.");
            return;
        }

        bool isPlayer = enemyTransform.name == "Player";

        TextMeshPro textMesh = Instantiate(damageTextMeshPrefab, enemyTransform.position, Quaternion.identity, DamageTextContainer.transform);


        if (isPlayer)
        {
            textMesh.text = "-" + Mathf.RoundToInt(amount).ToString();
            textMesh.color = Color.red;
        }
        else
        {
            textMesh.text = Mathf.RoundToInt(amount).ToString();
            textMesh.color = Color.white;
        }

        
        DamageTextContainer.GetComponent<MonoBehaviour>().StartCoroutine(AnimateDamageText(textMesh));
        


    }


    private IEnumerator AnimateDamageText(TextMeshPro textMesh)
    {
        Color startColor = textMesh.color;
        float startSize = textMesh.fontSize;
        float elapsedTime = 0f;

        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;


            textMesh.fontSize = Mathf.Lerp(startSize, startSize * 0.75f, progress);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1, 0, progress));

            yield return null;
        }

        Destroy(textMesh.gameObject);

   
    }









}


