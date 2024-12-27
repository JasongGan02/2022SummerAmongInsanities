using System;
using System.Collections;
using UnityEngine;

public class SoulController : MonoBehaviour
{
    public float initialSpeed = 4f;
    public float maxSpeed = 12f;
    public float acceleration = 2.5f;
    public float distanceThreshold = 0.6f;
    public float pickupDelay = 0.2f;
    public float timeSinceSpawn = 0f;
    private float experienceValue = 0;

    private bool shouldFlyToPlayer = false;
    private GameObject player;
    private PlayerExperience playerExperience;
    private float currentSpeed;

    public void Initialize(float experienceValue)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerExperience = player.GetComponent<PlayerExperience>();
        this.experienceValue = experienceValue;
        currentSpeed = initialSpeed;
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
        }

        if (transform.position.y < -100)
        {
            Destroy(gameObject);
        }
    }

    private void MoveTowardsPlayer()
    {
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, currentSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, player.transform.position) < distanceThreshold)
        {
            GrantExperience();
        }
    }
    
    public void StartTrackingPlayer()
    {
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<Collider2D>().enabled = false;
        shouldFlyToPlayer = true;
    }

    private void GrantExperience()
    {
        if (playerExperience == null)
        {
            Destroy(gameObject);
        }
        playerExperience.AddExperience(experienceValue);
        Destroy(gameObject);
    }
}
