using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedObjectController : MonoBehaviour
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
        inventory = player.GetComponent<Inventory>();
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

        inventory.AddItem(collectibleObject);
        Destroy(gameObject);
    }
}