using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreArchitecture : MonoBehaviour
{
    [SerializeField] float Constructable_Distance;
    [SerializeField] GameObject Constructable_Circle;
    Playermovement player;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Playermovement>();
        InitConstructableCircle();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        Vector3 playerPosition = player.gameObject.transform.position;
        Vector3 corePosition = gameObject.transform.position;

        float current_Distance = Mathf.Sqrt(Mathf.Pow((playerPosition.x-corePosition.x), 2) + Mathf.Pow((playerPosition.y-corePosition.y), 2));   
        if(current_Distance <= Constructable_Distance)
        {
            print("Player is in construction range");
            return true;
        }

        print("Player is out of construction range");
        return false;
    }
}
