using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedObjectController : MonoBehaviour
{
    public IInventoryObject item;
    public float speed = 0.2f;
    public float distanceThreshold = 3f;
    public int amount = 1;

    private bool shouldFlyToPlayer = false;
    private GameObject player;
    private Inventory inventory;

    public void Initialize(IInventoryObject item, int amount)
    {
        this.amount = amount;
        this.item = item;
        GameObject currentChunk = WorldGenerator.TotalChunks[WorldGenerator.GetChunkCoordsFromPosition(transform.position)];
        transform.SetParent(currentChunk.transform, true);
    }

    private void Start()
    {
        player = GameObject.Find(Constants.Name.PLAYER);
        inventory = FindObjectOfType<Inventory>();
    }

    private void FixedUpdate()
    {
        if (player==null)
        {
            player = GameObject.FindWithTag("Player");
        }
        else{
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

    }

    public void PickingUp()
    {
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<Collider2D>().enabled = false;
        shouldFlyToPlayer = true;
    }

    private void PickedUp()
    {
        shouldFlyToPlayer = false;

        inventory.AddItem(item, amount);
        if (item is ProjectileObject)
        {
            Projectile projectileComponent = GetComponent<Projectile>();
            GetComponent<Rigidbody2D>().simulated = true;
            GetComponent<Collider2D>().enabled = true;
            GetComponent<Collider2D>().isTrigger = true;
            if (projectileComponent != null)
            {
                projectileComponent.enabled = true;
            }
            ProjectilePoolManager.Instance.ReturnProjectile(gameObject, (item as BaseObject).getPrefab());
            this.enabled = false;
        }
        else
        {
            Destroy(gameObject);
        }
        
    }
}
