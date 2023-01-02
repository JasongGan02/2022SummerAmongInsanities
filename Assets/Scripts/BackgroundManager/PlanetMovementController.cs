using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetMovementController : MonoBehaviour
{
    [SerializeField] private float heightOffset = 14.1f;
    [SerializeField] private float height = 7f;
    [SerializeField] private float radius = 13f;
    [SerializeField] private float duration;
    private float speed;

    private Camera mainCamera;
    private TimeSystemManager timeSystemManager;

    private float timeCounter = -Mathf.PI / 2;
    private void Awake()
    {
        mainCamera = Camera.main;
        timeSystemManager = FindObjectOfType<TimeSystemManager>();

        speed = Mathf.PI / duration;
    }
    private void OnEnable()
    {
        timeCounter = -Mathf.PI / 2;
    }

    private void Update()
    {
        timeCounter += Time.deltaTime * speed;
        var x = Mathf.Sin(timeCounter) * radius;
        var y = Mathf.Cos(timeCounter) * height;
        transform.localPosition = new Vector2(x + mainCamera.transform.position.x, y + heightOffset);
    }
}
