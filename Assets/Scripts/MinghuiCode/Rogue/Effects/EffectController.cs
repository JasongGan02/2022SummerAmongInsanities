using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Threading;
using UnityEditor.Build;

public class EffectController : MonoBehaviour
{

    protected int cost;
    protected int level;
    protected float duration; // The duration of the effect in seconds
    protected bool stackable;
    protected EffectObject effectObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void Initialize(EffectObject effectObject)
    {
        Type controllerType = GetType();
        Type objectType = effectObject.GetType();
        this.effectObject = effectObject;

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
                controllerField.SetValue(this, objectField.GetValue(effectObject));
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
                controllerProperty.SetValue(this, objectProperty.GetValue(effectObject));
            }
        }
    }
}
