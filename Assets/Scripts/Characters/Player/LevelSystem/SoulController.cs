using System.Collections;
using UnityEngine;

public class SoulController : MonoBehaviour
{
    public float speed = 0.2f;
    public float distanceThreshold = 0.6f;
    public float floatAmplitude = 0.1f;
    public float floatFrequency = 1f;
    public float pickupDelay = 1f;
    public float timeSinceSpawn = 0f;
    private float experienceValue = 0; // Value of experience or resource the Soul gives

    private bool shouldFlyToPlayer = false;
    private GameObject player;
    private PlayerExperience playerExperience;
    private Vector3 originalPosition;

    public void Initialize(float experienceValue)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerExperience = player.GetComponent<PlayerExperience>();
        this.experienceValue = experienceValue;
        originalPosition = transform.position;
        UpdateChunk();
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
    private void FixedUpdate()
    {
        UpdateChunk();
        timeSinceSpawn += Time.fixedDeltaTime;
        
        if (timeSinceSpawn >= pickupDelay)
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }
            else
            {
                if (shouldFlyToPlayer)
                {
                    MoveTowardsPlayer();
                }
            }

            ApplyFloatingEffect();
        }

        // Destroy the object if it falls below a certain threshold
        if (transform.position.y < -100)
        {
            Destroy(gameObject);
        }
    }

    private void MoveTowardsPlayer()
    {
        transform.position = Vector2.Lerp(transform.position, player.transform.position, speed);
        if (Vector2.Distance(transform.position, player.transform.position) < distanceThreshold)
        {
            GrantExperience();
        }
    }

    private void ApplyFloatingEffect()
    {
        // Sinusoidal oscillation for floating effect
        float newY = originalPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(originalPosition.x, newY, originalPosition.z);
    }

    public void StartTrackingPlayer()
    {
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<Collider2D>().enabled = false;
        shouldFlyToPlayer = true;
    }

    private void GrantExperience()
    {
        playerExperience.AddExperience(experienceValue);
        Destroy(gameObject); // Remove the Soul after it's collected.
    }
}
