using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedObjectController : MonoBehaviour
{
    public IInventoryObject item;
    public string testName;
    public float speed = 0.2f;
    public float distanceThreshold = 0.1f;
    public int amount = 1;

    private bool shouldFlyToPlayer = false;
    private GameObject player;
    private Inventory inventory;

    public void Initialize(IInventoryObject item, int amount)
    {
        this.amount = amount;
        this.item = item;
        testName = item.GetItemName();
    }

    private void Start()
    {
        player = GameObject.Find(Constants.Name.PLAYER);
        inventory = player.GetComponent<Inventory>();
    }

    private void Update()
    {
        if (shouldFlyToPlayer)
        {
            // TODO should let player own this logic
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

        inventory.AddItem(item, amount);
        Destroy(gameObject);
    }
}
