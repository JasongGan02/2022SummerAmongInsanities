using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedObject : MonoBehaviour
{
    public CollectibleObject collectibleObject;
    public float speed = 0.2f;
    public float distanceThreshold = 0.1f;

    private bool shouldFlyToPlayer = false;
    private GameObject player;
    private Inventory inventory;

    private void Start()
    {
        player = GameObject.Find(Constants.Name.PLAYER);
    }

    private void Update()
    {
        if (shouldFlyToPlayer)
        {
            transform.position = Vector2.Lerp(transform.position, player.transform.position, speed);
            if (Vector2.Distance(transform.position, player.transform.position) < distanceThreshold)
            {
                PickedUp();
            }
        }
    }

    public void PickingUp()
    {
        Destroy(GetComponent<Rigidbody2D>());
        Destroy(GetComponent<BoxCollider2D>());
        shouldFlyToPlayer = true;
    }

    private void PickedUp()
    {
        shouldFlyToPlayer = false;

        Inventory inventory = player.GetComponent<Inventory>();
        inventory.AddItem(collectibleObject);
        Destroy(gameObject);
    }
}
