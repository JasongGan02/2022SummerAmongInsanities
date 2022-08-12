using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BreakableObject : MonoBehaviour
{

    public int healthPoint = 4;
    public int numberOfResources = 4;
    public GameObject resourcePrefab = null;
    public void OnClicked()
    {
        healthPoint -= 1;
        if (healthPoint <= 0)
        {
            OnObjectDestroyed();
            Destroy(this.gameObject);
        }
    }

    private void OnObjectDestroyed()
    {
        if (resourcePrefab != null)
        {
            for (int i = 0; i < numberOfResources; i++)
            {
                GameObject resourceGameObject = Instantiate(resourcePrefab);
                resourceGameObject.transform.parent = this.gameObject.transform.parent;
                resourceGameObject.transform.position = this.gameObject.transform.position;
                float sizeRatio = resourceGameObject.GetComponent<ResourceObject>().sizeRatio;
                resourceGameObject.transform.localScale = new Vector2(sizeRatio, sizeRatio);
                resourceGameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 50);
            }
        }
    }
}
