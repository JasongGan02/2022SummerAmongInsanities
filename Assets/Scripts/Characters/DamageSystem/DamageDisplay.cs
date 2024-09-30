using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using static UnityEditorInternal.ReorderableList;

public class DamageDisplay : MonoBehaviour
{
    private float floatUpDistance = 1f;
    private float duration = 1f;

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


        float minDamage = 1f;  // 最小伤害值
        float maxDamage = 20f;  // 最大伤害值
        float minFontSize = 3f;  // 最小字体大小
        float maxFontSize = 8f;  // 最大字体大小

        // 将伤害值映射到字体大小范围内
        textMesh.fontSize = Mathf.Lerp(minFontSize, maxFontSize, Mathf.InverseLerp(minDamage, maxDamage, amount));

        DamageTextContainer.GetComponent<MonoBehaviour>().StartCoroutine(AnimateDamageText(textMesh));
        


    }


    private IEnumerator AnimateDamageText(TextMeshPro textMesh)
    {
        Color startColor = textMesh.color;
        float startSize = textMesh.fontSize;

        float appearTime = 0.15f;  
        float holdTime = 0.75f;  
        float shrinkTime = duration - appearTime - holdTime; 

        float elapsedTime = 0f;

        Vector3 startPos = textMesh.transform.position + new Vector3(0, 0.3f, 0); ;
        Vector3 initialPos = startPos; 
        Vector3 floatUpPos = startPos + new Vector3(0, 0.6f, 0);  // 上移的目标位置（达到这个位置后发生刹车）

        textMesh.transform.position = initialPos;


        while (elapsedTime < appearTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / appearTime;

            textMesh.transform.position = Vector3.Lerp(initialPos, floatUpPos, Mathf.SmoothStep(0f, 1f, progress));

            yield return null;
        }


        float reboundMagnitude = 0.1f;  
        Vector3 reboundPos = floatUpPos + new Vector3(0, -reboundMagnitude, 0);  
        elapsedTime = 0f;
        float reboundTime = 0.1f; 

        while (elapsedTime < reboundTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / reboundTime;

            textMesh.transform.position = Vector3.Lerp(floatUpPos, reboundPos, progress);  

            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < reboundTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / reboundTime;

            textMesh.transform.position = Vector3.Lerp(reboundPos, floatUpPos, progress);  

            yield return null;
        }


        yield return new WaitForSeconds(holdTime);

        elapsedTime = 0f;
        while (elapsedTime < shrinkTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / shrinkTime;

            textMesh.fontSize = Mathf.Lerp(startSize, startSize * 0.75f, progress);  
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1, 0, progress));  

            yield return null;
        }

        Destroy(textMesh.gameObject);
    }












}


