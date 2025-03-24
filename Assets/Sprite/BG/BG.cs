using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BG : MonoBehaviour
{
    [Header("Parallax Settings")]
    public float parallaxEffectX = 1f;  
    public float parallaxEffectY = 1f; 

    [Header("X Axis Tiling")]
    public bool enableXTiling = true;  
    private float lengthX;           

    [Header("Y Axis Down Limit")]
    public float maxDownOffset = 10f;

    private Vector2 startPos;
    private GameObject cam;

    void Start()
    {
        cam = Camera.main.gameObject;
        startPos = transform.position;

        if (enableXTiling)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            lengthX = sr.bounds.size.x;
        }
    }

    void Update()
    {
        Vector2 camPos = cam.transform.position;

        float distX = camPos.x - startPos.x;
        float distY = camPos.y - startPos.y;

        float newX = startPos.x + distX * parallaxEffectX;

        if (enableXTiling)
        {
            float centerToBg = camPos.x - transform.position.x;
            if (Mathf.Abs(centerToBg) >= lengthX / 2f)
            {
                startPos.x += (centerToBg > 0) ? lengthX : -lengthX;
                newX = startPos.x + (camPos.x - startPos.x) * parallaxEffectX;
            }
        }

        float offsetY = distY * parallaxEffectY;      
        float newY = startPos.y + offsetY;

        float minY = startPos.y - maxDownOffset;
        if (newY < minY)
        {
            newY = minY;
        }

        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}
