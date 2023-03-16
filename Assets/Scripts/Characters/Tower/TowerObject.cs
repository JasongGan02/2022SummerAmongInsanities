using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Character Objects/Tower Object")]
public class TowerObject : CharacterObject
{
    public float bullet_speed;
    public GameObject shadowPrefab;
    public GameObject bullet;

    public virtual GameObject GetShadowTowerObject()
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        SpriteRenderer spriteRenderer = worldGameObject.GetComponent<SpriteRenderer>(); // Get the sprite renderer component
        Color spriteColor = spriteRenderer.color; // Get the current color of the sprite
        spriteColor.a = 100 / 255f; // Set the alpha value to 100 (out of 255)
        spriteRenderer.color = spriteColor; // Assign the new color back to the sprite renderer
        var controller = worldGameObject.AddComponent<ConstructionShadows>();
        return worldGameObject;
    }
}
