using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Bullets normally shoot by different kinds of tower
public class Bullet : MonoBehaviour
{
    [SerializeField] float liveTime;
    [SerializeField] bool isRotate;
    [SerializeField] int damage;
    
    // Start is called before the first frame update
    void Start()
    {
        SelfDestroy();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isRotate)
        {
            SelfRotate();
        }
    }
    
    void SelfDestroy()
    {
        Destroy(gameObject, liveTime);
    }

    void SelfRotate()
    {
        gameObject.transform.Rotate(Vector3.forward, 150*Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "enemy")
        {
            // cause damage
            other.gameObject.GetComponent<CharacterController>().takenDamage(damage);

            // delete bullet
            Destroy(gameObject);

        }
    }

}
