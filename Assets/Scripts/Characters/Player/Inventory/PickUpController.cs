using System.Collections;
using UnityEngine;

public abstract class PickupController : MonoBehaviour
{
    public float distanceThreshold = 0.6f;
    public float pickupDelay = 0.5f;
    public float timeSinceSpawn = 0f;

    public float initialSpeed = 4f;
    public float maxSpeed = 12f;
    public float acceleration = 2.5f;

    // Floating properties
    public float floatAmplitude = 0.06f; // Height of the floating motion
    public float floatFrequency = 1.2f; // Speed of the floating motion
    public float floatingDelay = 0.8f; // Delay before starting to float

    protected GameObject player;
    protected bool shouldFlyToPlayer = false;
    private SpriteRenderer spriteRenderer;

    private readonly float targetWorldSize = 0.35f;
    private float currentSpeed;
    private bool isGrounded = false;
    private bool isFloating = false;
    private bool isWaitingToFloat = false;
    private Vector3 initialPosition;


    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        currentSpeed = initialSpeed;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerID = SortingLayer.NameToID("DroppedObject");
        initialPosition = transform.position;
    }

    protected virtual void FixedUpdate()
    {
        timeSinceSpawn += Time.fixedDeltaTime;
        UpdateChunk();

        if (player == null)
        {
            FindFields();
        }
        else if (timeSinceSpawn >= pickupDelay && shouldFlyToPlayer && player != null)
        {
            MoveTowardsPlayer();
            return; // Skip floating if moving towards player
        }

        if (!isGrounded && IsOnGround())
        {
            isGrounded = true;
            if (!isWaitingToFloat)
            {
                StartCoroutine(StartFloatingAfterDelay());
            }
        }

        if (transform.position.y < -100)
        {
            HandleOutOfBound();
        }
    }

    protected virtual void FindFields()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    protected virtual void UpdateChunk()
    {
        if (WorldGenerator.TotalChunks.ContainsKey(WorldGenerator.GetChunkCoordsFromPosition(transform.position)))
        {
            GameObject currentChunk =
                WorldGenerator.TotalChunks[WorldGenerator.GetChunkCoordsFromPosition(transform.position)];
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

        // Stop floating if it starts flying to player
        StopFloating();
    }

    protected virtual void MoveTowardsPlayer()
    {
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        transform.position =
            Vector3.MoveTowards(transform.position, player.transform.position, currentSpeed * Time.deltaTime);

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
                Vector2 spriteSize = sprite.bounds.size;
                float maxDimension = Mathf.Max(spriteSize.x, spriteSize.y);
                float scaleFactor = targetWorldSize / maxDimension;
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

    private bool IsOnGround()
    {
        // Define the layer mask for the ground layer
        int groundLayerMask = LayerMask.GetMask("ground");

        // Perform a raycast only on the ground layer
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.35f, groundLayerMask);
        return hit.collider != null;
    }

    private IEnumerator StartFloatingAfterDelay()
    {
        isWaitingToFloat = true; // Prevent multiple delays from overlapping
        yield return new WaitForSeconds(floatingDelay);

        // Check if still grounded after the delay
        if (isGrounded && !isFloating)
        {
            StartFloating();
        }

        isWaitingToFloat = false; // Allow further floating attempts if needed
    }

    private void StartFloating()
    {
        if (!isFloating)
        {
            isFloating = true;
            Vector3 currentPosition = transform.position;
            float groundLevel = GetGroundLevel();
            initialPosition = new Vector3(currentPosition.x, Mathf.Max(currentPosition.y, groundLevel + 0.25f),
                currentPosition.z);

            StartCoroutine(FloatCoroutine());
        }
    }

    private float GetGroundLevel()
    {
        // Define the layer mask for the ground layer
        int groundLayerMask = LayerMask.GetMask("ground");

        // Perform a raycast only on the ground layer
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, groundLayerMask);
        if (hit.collider != null)
        {
            return hit.point.y; // Return the y-position of the hit point
        }

        return transform.position.y; // Default to current position if no ground detected
    }


    private void StopFloating()
    {
        isFloating = false;
    }

    private IEnumerator FloatCoroutine()
    {
        float elapsed = 0f;

        while (isFloating)
        {
            // Check if the object is no longer grounded
            if (!IsOnGround())
            {
                isGrounded = false;
                StopFloating();
                yield break; // Exit the coroutine if no longer grounded
            }

            elapsed += Time.deltaTime * floatFrequency;
            float offsetY = Mathf.Sin(elapsed) * floatAmplitude;
            transform.position = new Vector3(initialPosition.x, initialPosition.y + offsetY, initialPosition.z);
            yield return null;
        }
    }

    protected abstract bool CheckBeforePickingUp();
    protected abstract void HandleOutOfBound();
    protected abstract void OnPickup();
}