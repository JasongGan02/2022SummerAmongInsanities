using UnityEngine;

public class BG2 : MonoBehaviour
{
    private float length, height, startPosX, startPosY;
    public GameObject cam;
    public float horizontalParallaxEffect;
    public float verticalParallaxEffect;

    void Start()
    {
        startPosX = transform.position.x;
        startPosY = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        height = GetComponent<SpriteRenderer>().bounds.size.y;
        cam = Camera.main.gameObject;
    }

    void Update()
    {
        float tempX = (cam.transform.position.x * (1 - horizontalParallaxEffect));
        float distX = (cam.transform.position.x * horizontalParallaxEffect);

        float tempY = (cam.transform.position.y * (1 - verticalParallaxEffect));
        float distY = (cam.transform.position.y * verticalParallaxEffect);

        transform.position = new Vector3(startPosX + distX, startPosY + distY, transform.position.z);

        if (tempX > startPosX + length) startPosX += length;
        else if (tempX < startPosX - length) startPosX -= length;

        if (tempY > startPosY + height) startPosY += height;
        else if (tempY < startPosY - height) startPosY -= height;
    }
}
