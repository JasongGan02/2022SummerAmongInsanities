using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceObject : MonoBehaviour
{
    public string resourceType;
    public int amount = 1;
    public float sizeRatio = 0.5f;

    private bool shouldFlyToPlayer = false;
    private GameObject player;

    private void Start()
    {
        player = GameObject.Find(Constants.Name.PLAYER);
    }

    private void Update()
    {
        if (shouldFlyToPlayer)
        {
            transform.position = Vector2.Lerp(transform.position, player.transform.position, 0.1f);
            if (Vector2.Distance(transform.position, player.transform.position) < 0.05f)
            {
                OnPickedUp();
            }
        }
    }

    public void OnBeforePickedUp()
    {
        Destroy(GetComponent<Rigidbody2D>());
        Destroy(GetComponent<BoxCollider2D>());
        shouldFlyToPlayer = true;
    }

    private void OnPickedUp()
    {
        shouldFlyToPlayer = false;

        Inventory inventory = player.GetComponent<Inventory>();
        inventory.AddItem(this);
        Destroy(transform.gameObject);
    }
}
