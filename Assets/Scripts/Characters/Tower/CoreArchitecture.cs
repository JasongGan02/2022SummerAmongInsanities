using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class CoreArchitecture : MonoBehaviour
{
    [SerializeField] float Constructable_Distance;
    [SerializeField] GameObject Constructable_Circle;
    private CoreArchitecture coreArchitecture;
    private Inventory inventory;
    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        InitConstructableCircle();
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
        inventory = FindObjectOfType<Inventory>();
    }

    // Update is called once per frame
    void Update()
    {
        if(player == null)
            player = GameObject.FindWithTag("Player");
    }


    public void spawnItems(BaseObject item)
    {
        StartCoroutine(Spawn(item));
    }


    public System.Collections.IEnumerator Spawn(BaseObject item)
    {
        yield return new WaitForSeconds(5);
        GameObject spawnObject = Instantiate(item.getPrefab(), coreArchitecture.transform.position + new Vector3(1,0,0), Quaternion.Euler(new Vector3(0, 0, 90)));
        spawnObject.layer = LayerMask.NameToLayer("resource");
        var controller = spawnObject.AddComponent<DroppedObjectController>();
        controller.Initialize(item as IInventoryObject, 1);
        yield return new WaitForSeconds(5);

    }

    void InitConstructableCircle()
    {
        Constructable_Circle.gameObject.transform.localScale = new Vector3(Constructable_Distance*2, Constructable_Distance*2, 1);
        CloseConstructionMode();
    }

    public void OpenConstructionMode()
    {
        Constructable_Circle.SetActive(true);
    }

    public void CloseConstructionMode()
    {
        Constructable_Circle.SetActive(false);
    }

    public bool IsPlayerInControlRange()
    {
        if(player == null)
        {
            return false;
        }
            
        Vector3 playerPosition = player.transform.position;
        Vector3 corePosition = gameObject.transform.position;
        
        
        float current_Distance = Mathf.Sqrt(Mathf.Pow((playerPosition.x-corePosition.x), 2) + Mathf.Pow((playerPosition.y-corePosition.y), 2));   
        if(current_Distance <= Constructable_Distance)
        {
            //print("Player is in construction range");
            return true;
        }

        //print("Player is out of construction range");
        return false;
    }

    public float GetConstructableDistance()
    {
        return Constructable_Distance;
    }

}
