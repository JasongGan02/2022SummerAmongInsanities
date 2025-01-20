using UnityEngine;

public class BG : MonoBehaviour
{
    private Vector2 startPos;
    private Vector2 travel => (Vector2)cam.transform.position - startPos;
    private GameObject cam;
    public float parallaxEffect;

    void Start()
    {
        startPos = transform.position;
        cam = Camera.main.gameObject;
    }

    void Update()
    {
        Vector2 newPos = startPos + travel * parallaxEffect;
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
    }
}
