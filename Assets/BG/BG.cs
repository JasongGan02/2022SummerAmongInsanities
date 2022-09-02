using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BG : MonoBehaviour
{
    private float length, startpos;
    public GameObject cam;
    public float parallaxEffect;

    void Start()
    {
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;

    }


    void Update()
    {
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dis = (cam.transform.position.x * parallaxEffect);
        transform.position = new Vector3(startpos + dis, transform.position.y, transform.position.z);

        if (temp > (startpos + length))
        {
            startpos += length;
        }
        else if (temp < (startpos - length))
        {
            startpos -= length;
        }

    }

}
