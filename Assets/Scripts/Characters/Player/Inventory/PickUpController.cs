using UnityEngine;

public abstract class PickupController : MonoBehaviour
{
    public float distanceThreshold = 0.6f;
    public float pickupDelay = 0.5f;
    public float timeSinceSpawn = 0f;

    public float initialSpeed = 4f;
    public float maxSpeed = 12f;
    public float acceleration = 2.5f;

    protected GameObject player;
    protected bool shouldFlyToPlayer = false;
    private readonly float targetWorldSize = 0.35f;
    private float currentSpeed;
    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        currentSpeed = initialSpeed;
        GetComponent<SpriteRenderer>().sortingOrder = 11;
    }

    protected virtual void FixedUpdate()
    {
        timeSinceSpawn += Time.fixedDeltaTime;
        UpdateChunk();
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        else if (timeSinceSpawn >= pickupDelay && shouldFlyToPlayer && player != null)
        {
            MoveTowardsPlayer();
        }

        if (transform.position.y < -100)
        {
            HandleOutOfBound();
        }
    }

    protected virtual void UpdateChunk()
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

    public virtual void StartTrackingPlayer()
    {
        if (!CheckBeforePickingUp()) return;
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<Collider2D>().enabled = false;
        shouldFlyToPlayer = true;
    }

    protected virtual void MoveTowardsPlayer()
    {
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, currentSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, player.transform.position) < distanceThreshold)
        {
            OnPickup();
        }
    }
    
    protected virtual void NormalizeObjectSize()
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
    
    protected abstract bool CheckBeforePickingUp(); 
    protected abstract void HandleOutOfBound(); 
    protected abstract void OnPickup(); // To define specific pickup behavior
}
