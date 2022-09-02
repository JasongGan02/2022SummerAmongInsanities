using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemClickHandler : MonoBehaviour, IPointerClickHandler
{
    public CollectibleObject collectibleItem;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (collectibleItem == null) return;
        Debug.Log("using " + collectibleItem.itemName);
        /*switch(collectibleItem.itemType)
        {
            case ItemType.Comsumable:
                Destroy(gameObject);
                break;
            case 
        }*/
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
