using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using static UnityEditorInternal.ReorderableList;
using System.Collections.Generic;

public class DamageDisplay : MonoBehaviour
{
    private float floatUpDistance = 1f;
    private float duration = 1f;

    private Color RedColor = Color.red;
    private Color GreenColor = Color.green;
    private Color DefaultColor = Color.white;
    private Dictionary<System.Type, Color> damageSourceColors = new Dictionary<System.Type, Color>();
    private GameObject DamageTextContainer;


    public void ShowDamage(float amount,Transform enemyTransform, float Health, IDamageSource damageSource)
    {
        DamageTextContainer = GameObject.Find("DamageTextContainer");
        var damageTextMeshPrefab = Resources.Load<TextMeshPro>("UI/DamageDisplay");

        if (damageTextMeshPrefab == null)
        {
            Debug.LogError("Damage Text Mesh Prefab not found in Resources. Please check the path.");
            return;
        }
        Color damageColor = GetColorForDamageSource(damageSource);

        bool isPlayer = enemyTransform.CompareTag("Player");
        bool isTower = enemyTransform.CompareTag("tower");

        TextMeshPro textMesh = Instantiate(damageTextMeshPrefab, enemyTransform.position, Quaternion.identity, DamageTextContainer.transform);


        if (amount < 0)
        {
            textMesh.text = "+" + Mathf.Abs(Mathf.RoundToInt(amount)).ToString(); 
            textMesh.color = GreenColor;
        }
        else
        {
            if (isPlayer || isTower)
            {
                textMesh.text = "-" + Mathf.RoundToInt(amount).ToString();
                textMesh.color = RedColor; 
            }
            else
            {
                textMesh.text = Mathf.RoundToInt(amount).ToString();
                textMesh.color = damageColor;
            }
        }


       
        float damageRatio = amount / Health;  
        float minFontSize = 3f;  
        float maxFontSize = 9f;  

        textMesh.fontSize = Mathf.Lerp(minFontSize, maxFontSize, damageRatio);


        DamageTextContainer.GetComponent<MonoBehaviour>().StartCoroutine(AnimateDamageText(textMesh, enemyTransform.position));



    }

    private Color GetColorForDamageSource(IDamageSource damageSource)
    {
        

        System.Type sourceType = damageSource.GetType();

        if (!damageSourceColors.ContainsKey(sourceType))
        {
            if (sourceType == typeof(OnFireEffectController))
            {
                damageSourceColors[sourceType] = Color.yellow; 
            }
            else
            {
                return DefaultColor;
            }
        }

        return damageSourceColors[sourceType];
    }



    private IEnumerator AnimateDamageText(TextMeshPro textMesh, Vector3 centerPosition)
    {
        Color startColor = textMesh.color;
        float startSize = textMesh.fontSize;

        float appearTime = 0.15f;
        float holdTime = 0.35f;
        float shrinkTime = duration - appearTime - holdTime;

        float elapsedTime = 0f;

        Vector3 randomStartOffset = Random.insideUnitCircle * 0.5f;
        Vector3 randomEndOffset = Random.insideUnitCircle * 0.7f; 

        Vector3 startPos = centerPosition + randomStartOffset;
        Vector3 floatUpPos = centerPosition + randomEndOffset + new Vector3(0, 1f, 0); 

        textMesh.transform.position = startPos;


        float parabolaHeight = 0.7f;
        float parabolaDuration = appearTime + holdTime; 

 
        while (elapsedTime < parabolaDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / parabolaDuration;
            float heightOffset = 4 * parabolaHeight * progress * (1 - progress); 
            Vector3 position = Vector3.Lerp(startPos, floatUpPos, progress);
            position.y += heightOffset; 
            textMesh.transform.position = position;

            yield return null;
        }

     
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


