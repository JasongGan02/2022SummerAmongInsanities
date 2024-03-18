using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearController : MonoBehaviour
{
    private RectTransform rectTransform;
    [SerializeField] private float rotationSpeed = 100f; // degrees per second
    private bool isRotating = true;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }   

    

    // Update is called once per frame
    void Update()
    {
        // Check if gears should be rotating
        if (isRotating)
        {
            rectTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }

    // Method to start gear rotation
    public void StartRotation()
    {
        isRotating = true;
    }

    // Method to stop gear rotation
    public void StopRotation()
    {
        isRotating = false;
    }
}
