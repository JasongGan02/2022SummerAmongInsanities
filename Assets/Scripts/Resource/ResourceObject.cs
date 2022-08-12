using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceObject : MonoBehaviour
{
    public string resourceType;
    public int amount = 1;
    public float sizeRatio = 0.5f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == Constants.Tag.PLAYER)
        {
            Destroy(this.gameObject);
            // TODO: Add amount to the resourceType for the player's intentory 
        }
    }
}
