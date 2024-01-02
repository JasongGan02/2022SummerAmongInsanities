using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchController : MonoBehaviour
{
    protected GameObject player;
    protected Playermovement playermovement;
    protected PlayerInteraction playerinteraction;
    protected Inventory inventory;

    protected TorchObject TorchStats;
    Light2D TorchLight;
    Light2D GlobalLight;
    protected ShadowGenerator shadowGenerator;
    protected int skyLightHeight = -1;
    protected int lightRadius = -1;
    protected float outer;

    // Start is called before the first frame update
    public virtual void Start()
    {
        player = GameObject.Find("Player");
        playermovement = player.GetComponent<Playermovement>();
        playerinteraction = player.GetComponent<PlayerInteraction>();
        inventory = FindObjectOfType<Inventory>();
    }

    // Update is called once per frame
    public void Update()
    {
        Flip();

        Patrol();

        UseItem();
    }

    public virtual void Initialize(TorchObject TorchObject)
    {
        this.TorchStats = TorchObject;
        Type controllerType = GetType();
        Type objectType = TorchStats.GetType();

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
                controllerField.SetValue(this, objectField.GetValue(TorchObject));
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
                controllerProperty.SetValue(this, objectProperty.GetValue(TorchObject));
            }
        }
    }

    public virtual void UseItem()
    {
        if (TorchLight == null) { TorchLight = GetComponent<Light2D>(); }
        if (GlobalLight == null) { GlobalLight = GameObject.Find("BackgroundLight").GetComponent<Light2D>(); }
        if (shadowGenerator == null) { shadowGenerator = GameObject.Find("ShadowOverlay").GetComponent<ShadowGenerator>(); }
        if (lightRadius == -1) { lightRadius = shadowGenerator.lightRadius; }

        if (player.GetComponent<PlayerInteraction>() != null)
        {
            if (player.GetComponent<PlayerInteraction>().prop.GetComponent<TorchController>() != null)
            {
                /*if (GlobalLight.intensity > 0.5f)   // day time
                {
                    if (transform.position.y * 4 < skyLightHeight - lightRadius - 2)  // if underground
                    {
                        TorchLight.intensity = 0.8f;
                        AutoCleanShadow();
                    }
                    else
                    {
                        TorchLight.intensity = 0f;
                    }
                }  
                else                                // night 
                {
                    TorchLight.intensity = 0.8f;
                    AutoCleanShadow();
                } */
                TorchLight.intensity = 0.6f;
                AutoCleanShadow();
            }
            else
            {
                TorchLight.intensity = 0f;
            }
        }
        else 
        {
            TorchLight.intensity = 0f;
        }
    }

    public virtual void AutoCleanShadow()
    {
        if (outer == 0)
        {
            outer = GetComponent<Light2D>().pointLightInnerRadius * 4f;
        }
        if (shadowGenerator != null)
        {
            Debug.Log("clean distance is " + outer);
            shadowGenerator.LightAutoCleanShadow(transform.position.x, transform.position.y, outer);
        }
    }

    public virtual void Patrol()
    {
        if (playermovement.facingRight)
        {
            transform.position = player.transform.position + new Vector3(0.8f, 0, 0);
        }
        else
        {
            transform.position = player.transform.position - new Vector3(0.8f, 0, 0);
        }
    }

    public virtual void Flip()
    {
        if (playermovement.facingRight)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 315));
        }
        else
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 135));
        }
    }
}
