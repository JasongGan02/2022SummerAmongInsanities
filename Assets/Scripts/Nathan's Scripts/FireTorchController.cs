using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class FireTorchController : MonoBehaviour
{
    protected GameObject player;
    protected Playermovement playermovement;
    protected PlayerInteraction playerinteraction;
    protected Inventory inventory;

    protected FireTorchObject FireTorchStats;


    // Start is called before the first frame update
    public virtual void Start()
    {
        player = GameObject.Find("Player");
        playermovement = player.GetComponent<Playermovement>();
        playerinteraction = player.GetComponent<PlayerInteraction>();
        inventory = FindObjectOfType<Inventory>();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        Flip();

        if (Input.GetMouseButton(0))
        {
            UseItem();
        }
        else
        {
            PatrolItem();
        }
    }

    public virtual void Initialize(FireTorchObject fireTorchObject)
    {
        this.FireTorchStats = fireTorchObject;
        Type controllerType = GetType();
        Type objectType = FireTorchStats.GetType();

        // Get all the fields of the controller type
        FieldInfo[] controllerFields = controllerType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        // Iterate over the fields and set their values from the object
        foreach (FieldInfo controllerField in controllerFields)
        {
            // Try to find a matching field in the object
            FieldInfo objectField = objectType.GetField(controllerField.Name);
            // If there's a matching field, set the value
            if (objectField != null && objectField.FieldType == controllerField.FieldType)
            {
                Debug.Log(objectField.Name);
                Debug.Log(objectField.GetValue(fireTorchObject));
                controllerField.SetValue(this, objectField.GetValue(fireTorchObject));
            }
        }

        // Get all the properties of the controller type
        PropertyInfo[] controllerProperties = controllerType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);

        // Iterate over the properties and set their values from the object
        foreach (PropertyInfo controllerProperty in controllerProperties)
        {
            // Try to find a matching property in the object
            PropertyInfo objectProperty = objectType.GetProperty(controllerProperty.Name);

            // If there's a matching property, set the value
            if (objectProperty != null && objectProperty.PropertyType == controllerProperty.PropertyType && objectProperty.CanRead)
            {
                controllerProperty.SetValue(this, objectProperty.GetValue(fireTorchObject));
            }
        }
    }

    public virtual void UseItem()
    {

    }
    public virtual void PatrolItem()
    {
        transform.position = player.transform.position;
        //Vector2 newPosition = transform.position;
        //newPosition.x = player.transform.position.x - 0.2f;
        //transform.position = newPosition;
    }
    public virtual void Flip()
    {
        if (playermovement.facingRight && (transform.localScale.y < 0) || !playermovement.facingRight && (transform.localScale.y > 0))
        {
            Vector3 transformScale = transform.localScale;
            transformScale.x *= -1;
            transform.localScale = transformScale;
        }
    }
}
