﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour {

    public float speed;
    float inputX;
    float inputZ;
	
	// Update is called once per frame
	void Update () {
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
                Camera.main.orthographicSize = Mathf.Min(Camera.main.orthographicSize - 1, 100);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                Camera.main.orthographicSize = Mathf.Min(Camera.main.orthographicSize + 1, 100);

            }
        }
        
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 1f, 7f);
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