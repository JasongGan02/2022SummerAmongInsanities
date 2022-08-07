using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BreakableTile : MonoBehaviour
{

    public int healthPoint = 4;

    public void OnClicked()
    {
        healthPoint -= 1;
        if (healthPoint <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
