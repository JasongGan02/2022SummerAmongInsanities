using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedObjectController : MonoBehaviour
{
    public IInventoryObject item;
    public float speed = 0.2f;
    public float distanceThreshold = 0.6f;
    public int amount = 1;
    public float pickupDelay = 1f;
    public float timeSinceDrop = 0f;
    
    private bool shouldFlyToPlayer = false;
    private GameObject player;
    private Inventory inventory;
    private readonly float targetWorldSize = 0.25f;
    
    // Floating effect parameters
    public float floatAmplitude = 0.1f;  // How far up and down the object moves
    public float floatFrequency = 1f;    // How fast the object oscillates

    private Vector3 originalPosition;
    
    public void Initialize(IInventoryObject item, int amount)
    {
        this.amount = amount;
        this.item = item;
        UpdateChunk();
        NormalizeObjectSize();
        GetComponent<SpriteRenderer>().sortingOrder = 5;
    }

    private void UpdateChunk()
    {
        if (WorldGenerator.TotalChunks.ContainsKey(WorldGenerator.GetChunkCoordsFromPosition(transform.position)))
        {
            GameObject currentChunk = WorldGenerator.TotalChunks[WorldGenerator.GetChunkCoordsFromPosition(transform.position)];
            transform.SetParent(currentChunk.transform, true);
        }
        else
        {
            
            Debug.LogError("Drop Position's chunk is not initialized yet");
        }
    }

    private void Start()
    {
        player = GameObject.Find(Constants.Name.PLAYER);
        inventory = FindObjectOfType<Inventory>();
    }

    private void FixedUpdate()
    {
        timeSinceDrop += Time.fixedDeltaTime;
        UpdateChunk();
        //ApplyFloatingEffect();
        if (player==null)
        {
            player = GameObject.FindWithTag("Player");
        }
        else{
            if (shouldFlyToPlayer && timeSinceDrop >= pickupDelay)
            {
                // TODO should let player own this logic
                transform.position = Vector2.Lerp(transform.position, player.transform.position, speed);
                if (Vector2.Distance(transform.position, player.transform.position) < distanceThreshold)
                {
                    PickedUp();
                }
            }
        }
        if (GetComponent<Transform>().position.y < -100)
        {
            if (item is IPoolableObject)
            {
                Projectile projectileComponent = GetComponent<Projectile>();
                GetComponent<Rigidbody2D>().simulated = true;
                GetComponent<Collider2D>().enabled = true;
                GetComponent<Collider2D>().isTrigger = true;
                if (projectileComponent != null)
                {
                    projectileComponent.enabled = true;
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
        if (item is IPoolableObject)
        {
            Projectile projectileComponent = GetComponent<Projectile>();
            GetComponent<Rigidbody2D>().simulated = true;
            GetComponent<Collider2D>().enabled = true;
            GetComponent<Collider2D>().isTrigger = true;
            if (projectileComponent != null)
            {
                projectileComponent.enabled = true;
            }
            PoolManager.Instance.Return(gameObject, item as BaseObject);
            Destroy(this);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }
    
    private void NormalizeObjectSize()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Sprite sprite = spriteRenderer.sprite;
            if (sprite != null)
            {
                // Get the sprite's current size in world units
                Vector2 spriteSize = sprite.bounds.size; // World size of the sprite in units
            
                // Find the largest dimension of the sprite (width or height)
                float maxDimension = Mathf.Max(spriteSize.x, spriteSize.y);
            
                // Calculate the scale factor to make the largest dimension equal to targetWorldSize
                float scaleFactor = targetWorldSize / maxDimension;
            
                // Apply the scale factor to the object's local scale
                transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
            }
            else
            {
                Debug.LogError("Sprite is missing on the object.");
            }
        }
        else
        {
            Debug.LogError("SpriteRenderer is missing on the object.");
        }
    }
    
    private void ApplyFloatingEffect()
    {
        // Sinusoidal oscillation for floating effect
        float newY = originalPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(originalPosition.x, newY, originalPosition.z);
    }
}
