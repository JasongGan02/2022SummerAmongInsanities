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
    protected string description;
    protected EffectObject effectObject;

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
                Debug.Log(objectField.Name);
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

        StartEffect();
    }

    protected virtual void StartEffect()
    {
        // Start the effect or perform any necessary setup

        // Start a coroutine to wait for the specified duration and then destroy the component
        StartCoroutine(DestroyAfterDuration());
    }

    protected virtual System.Collections.IEnumerator DestroyAfterDuration()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Perform any necessary updates here if the effect is not a one-time effect
 
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //yield return new WaitForSeconds(duration);

        // Reset any temporary effect-related stats or variables here

        // Destroy the component after the duration has elapsed
        Destroy(this);
    }
}
