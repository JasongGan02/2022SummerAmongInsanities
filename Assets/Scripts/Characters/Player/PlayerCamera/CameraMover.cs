﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraMover : MonoBehaviour {

    public float speed;
    float inputX;
    float inputZ;
     private UnityEngine.Rendering.Universal.PixelPerfectCamera pixelPerfectCamera;

    private void Start()
    {
        // Get the reference to the PixelPerfectCamera component on the main camera
        pixelPerfectCamera = Camera.main.GetComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");

        if (inputX != 0)
            moveX();
        if (inputZ != 0)
            moveZ();

        if (!PlayerStatusRepository.GetIsViewingUi())
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
            {
                // Decrease assetsPPU to zoom out
                pixelPerfectCamera.assetsPPU += 4;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                // Increase assetsPPU to zoom in
                pixelPerfectCamera.assetsPPU -= 4;
            }
        }

        // Clamp the assetsPPU to a reasonable range (adjust the range as needed)
        pixelPerfectCamera.assetsPPU = Mathf.Clamp(pixelPerfectCamera.assetsPPU, 8, 40);
    }


        void moveZ()
        {
            transform.position += transform.up * inputZ * speed * Time.deltaTime;
        }


        void moveX()
        {
            transform.position += transform.right * inputX * speed * Time.deltaTime;
        }

}
