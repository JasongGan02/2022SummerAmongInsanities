using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightBackgroundController : MonoBehaviour
{

    private Camera mainCamera;
    // Start is called before the first frame update
    void Awake()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.y);
    }
}
