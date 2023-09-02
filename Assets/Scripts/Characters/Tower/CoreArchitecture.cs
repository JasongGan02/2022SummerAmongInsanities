using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class CoreArchitecture : MonoBehaviour
{
    [SerializeField] float Constructable_Distance;
    [SerializeField] GameObject Constructable_Circle;

    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        InitConstructableCircle();

    }

    // Update is called once per frame
    void Update()
    {
        if(player == null)
            player = GameObject.FindWithTag("Player");
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
