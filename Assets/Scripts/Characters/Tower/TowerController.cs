using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerController : CharacterController
{
    protected TowerStats TowerStats => (TowerStats)currentStats;

    //run-time variables
    protected ConstructionModeManager constructionModeManager;
    protected bool isDestroyedByPlayer = false;

    protected override void Update()
    {
        TestDrop();
    }

    protected override void Die()
    {
        constructionModeManager = FindObjectOfType<ConstructionModeManager>();
        constructionModeManager.ConsumeEnergy(TowerStats.energyCost * -1);
        Destroy(gameObject);
        OnObjectReturned(isDestroyedByPlayer);
    }


    public void TestDrop()
    {
        if (Input.GetMouseButtonDown(1)) // Check if right mouse button was clicked
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Create a ray from the mouse position
            RaycastHit2D hitInfo = Physics2D.Raycast(mousePosition, Vector2.zero); // Raycast from the mouse position
            Collider2D collider = GetComponent<Collider2D>(); // Get the collider of the object you want to check
            if (hitInfo.collider != null && hitInfo.collider == collider) // Check if the collider was hit by the ray
            {
                isDestroyedByPlayer = true;
                // Execute your function here
                Die();
            }
        }
    }
}