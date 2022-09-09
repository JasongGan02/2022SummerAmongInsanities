using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float liveTime;
    
    // Start is called before the first frame update
    void Start()
    {
        SelfDestroy();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void SelfDestroy()
    {
        Destroy(gameObject, liveTime);
    }
}
