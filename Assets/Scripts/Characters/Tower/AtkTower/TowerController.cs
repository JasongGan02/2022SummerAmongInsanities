using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerController : CharacterController
{
    //SO variables
    protected int energyCost;
    protected Quaternion rotateAngle;
    
    //run-time variables
    protected ConstructionMode constructionMode;
    protected bool isDestroyedByPlayer =false;

    protected override void Update()
    {
        base.Update();
        TestDrop();
    }
    //protected abstract void TowerLoop(); 

    public override void Reinitialize()
    {
        base.Reinitialize();

    }
    protected override void death()
    {
        constructionMode = FindObjectOfType<ConstructionMode>();
        constructionMode.EnergyConsumption(energyCost*-1);
        Destroy(gameObject);
        OnObjectReturned(isDestroyedByPlayer);
    }

    public override void ApplyHPChange(float dmg)
    {
        _HP -= dmg;

        if (_HP <= 0)
        {
            death();
        }
        if (_HP > characterStats._HP) //hp cap
        {
            _HP = characterStats._HP;
        }
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
                death();
            }
        }
    }

}
