using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "Scriptable Objects/Character Objects/Tower Object")]
public class TowerObject : CharacterObject, IShadowObject
{
    [Header("TowerObject Fields")]
    [HideInDerivedInspector]
    public TowerStats towerStats;

    public Sprite towerIcon;
    [SerializeField] Vector2Int colliderSize;
    [SerializeField] private bool isUnlocked;
    public CraftRecipe[] recipe;

    public bool IsUnlocked => isUnlocked;

    protected override void OnEnable()
    {
        baseStats = towerStats; // Ensure the baseStats is set
        base.OnEnable();
    }

    protected void CharacterOnEnable()
    {
        base.OnEnable();
    }

    public bool HasEnoughMaterials(Inventory inventory)
    {
        return isUnlocked && inventory.CheckRecipeHasEnoughMaterials(recipe);
    }

    public override GameObject GetPoolGameObject()
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        if (itemName.Contains("Wall"))
        {
            controllerName = "WallController";
        }
        else
        {
            controllerName = itemName + "Controller";
        }

        Type type = Type.GetType(controllerName);
        var controller = worldGameObject.AddComponent(type);
        (controller as CharacterController).Initialize(this);
        controller.gameObject.transform.parent = GameObject.Find("TowerContainer").transform;
        worldGameObject.transform.rotation = towerStats.curAngle;
        towerStats.curAngle = Quaternion.Euler(0, 0, 0);
        return worldGameObject;
    }


    /**
     * implementation of ConstructionMode
     */
    public virtual GameObject GetShadowGameObject()
    {
        var ghost = Instantiate(prefab);
        ghost.name = itemName;
        ghost.layer = Constants.Layer.DEFAULT;
        SpriteRenderer spriteRenderer = ghost.GetComponent<SpriteRenderer>(); // Get the sprite renderer component
        Color spriteColor = spriteRenderer.color; // Get the current color of the sprite
        spriteColor.a = 100 / 255f; // Set the alpha value to 100 (out of 255)
        spriteRenderer.color = spriteColor; // Assign the new color back to the sprite renderer
        var collider = ghost.GetComponent<BoxCollider2D>();
        collider.isTrigger = true;

        collider.size = new Vector2(collider.size.x * 0.9f, collider.size.y * 0.9f);
        ghost.AddComponent<ShadowObjectController>();
        var controller = ghost.GetComponent<Rigidbody2D>();
        controller.simulated = true;
        controller.isKinematic = true; //so that it does not fall and collide with everything

        return ghost;
    }

    public override GameObject GetSpawnedGameObject() //Use this when you are unsure about what type of controller will be using.
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        if (itemName.Contains("Wall"))
        {
            controllerName = "TowerController";
        }
        else
        {
            controllerName = itemName + "Controller";
        }

        Type type = Type.GetType(controllerName);
        var controller = worldGameObject.AddComponent(type);
        if (controller is CharacterController)
            (controller as CharacterController).Initialize(this);
        controller.gameObject.transform.parent = GameObject.Find("TowerContainer").transform;
        worldGameObject.transform.rotation = towerStats.curAngle;
        towerStats.curAngle = Quaternion.Euler(0, 0, 0);
        return worldGameObject;
    }
}