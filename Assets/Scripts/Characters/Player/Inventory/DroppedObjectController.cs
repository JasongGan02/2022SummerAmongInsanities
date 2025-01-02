using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedObjectController : PickupController
{
    private int amount = 1;
    private IInventoryObject item;
    private Inventory inventory;
    
    protected override void Start()
    {
        base.Start();
        inventory = FindObjectOfType<Inventory>();
    }
    
    public void Initialize(IInventoryObject item, int amount)
    {
        this.amount = amount;
        this.item = item;
        UpdateChunk();
        NormalizeObjectSize();
    }

    protected override bool CheckBeforePickingUp()
    {
        return inventory.CanAddItem(item);
    }
    
    protected override void OnPickup()
    {
        shouldFlyToPlayer = false;      

        inventory.AddItem(item, amount);
        if (item is IPoolableObject)
        {
            ProjectileController projectileControllerComponent = GetComponent<ProjectileController>();
            GetComponent<Rigidbody2D>().simulated = true;
            GetComponent<Collider2D>().enabled = true;
            GetComponent<Collider2D>().isTrigger = true;
            if (projectileControllerComponent != null)
            {
                projectileControllerComponent.enabled = true;
            }
            PoolManager.Instance.Return(gameObject, item as BaseObject);
            Destroy(this);
        }
        else
        {
            Destroy(gameObject);
        }
        player.GetComponent<CharacterController>().GetAudioEmitter().PlayClipFromCategory("ItemPickUp");
    }

    protected override void HandleOutOfBound()
    {
        if (item is IPoolableObject)
        {
            ProjectileController projectileControllerComponent = GetComponent<ProjectileController>();
            GetComponent<Rigidbody2D>().simulated = true;
            GetComponent<Collider2D>().enabled = true;
            GetComponent<Collider2D>().isTrigger = true;
            if (projectileControllerComponent != null)
            {
                projectileControllerComponent.enabled = true;
            }
            PoolManager.Instance.Return(gameObject, item as BaseObject);
            Destroy(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
