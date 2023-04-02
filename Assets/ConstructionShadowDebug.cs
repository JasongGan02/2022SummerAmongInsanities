using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionShadowDebug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.gameObject);
    
    }

    public void OnTriggerExit2D(Collider2D other)
    {
    }
}
