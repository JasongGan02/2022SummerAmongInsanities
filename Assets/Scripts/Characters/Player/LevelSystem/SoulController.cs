using System;
using System.Collections;
using UnityEngine;

public class SoulController : MonoBehaviour
{
    public float initialSpeed = 4f;
    public float maxSpeed = 12f;
    public float acceleration = 2.5f;
    public float distanceThreshold = 0.6f;
    public float floatAmplitude = 0.1f;
    public float floatFrequency = 1f;
    public float pickupDelay = 0.2f;
    public float timeSinceSpawn = 0f;
    public float hoverHeight = 0.5f; // Height to float above the ground
    private float experienceValue = 0;

    private bool shouldFlyToPlayer = false;
    private GameObject player;
    private PlayerExperience playerExperience;
    private Vector3 originalPosition;
    private float currentSpeed;
    private Rigidbody rb;

    public void Initialize(float experienceValue)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerExperience = player.GetComponent<PlayerExperience>();
        this.experienceValue = experienceValue;
        originalPosition = transform.position;
        currentSpeed = initialSpeed;
        rb = GetComponent<Rigidbody>();
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

    private void Update()
    {
        //if (!shouldFlyToPlayer)
            //ApplyFloatingEffect();
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

    private void ApplyFloatingEffect()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down,out hit))
        {
            float targetY = hit.point.y + hoverHeight + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            Vector3 targetPosition = new Vector3(transform.position.x, targetY, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * floatFrequency);
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
        playerExperience.AddExperience(experienceValue);
        Destroy(gameObject);
    }
}
