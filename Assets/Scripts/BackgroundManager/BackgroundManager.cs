using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private GameObject background;
    private GameObject sun;
    private GameObject moon;
    private GameObject redMoon;
    private GameObject backgroundLight;

    private TimeSystemManager timeSystemManager;
    private WorldGenerator worldGenerator;

    private Transform[] backgrounds; 
    private float[] parallaxScales; 
    public float smoothing = 1f;
    public float scrollSpeed = 0.5f;
    public float backgroundWidth = 20f;

    private Transform cam; 
    private Vector3 previousCamPos; 

    private Vector3 currentCamPos;

    void Awake()
    {
        GameObject BG = background.transform.Find(Constants.Name.BACKGROUND).gameObject;

        sun = BG.transform.Find(Constants.Name.SUN).gameObject;
        moon = BG.transform.Find(Constants.Name.MOON).gameObject;
        redMoon = BG.transform.Find(Constants.Name.RED_MOON).gameObject;
        backgroundLight = background.transform.Find(Constants.Name.BACKGROUND_LIGHT).gameObject;
        timeSystemManager = TimeSystemManager.Instance;
        worldGenerator = FindObjectOfType<WorldGenerator>();

        BG.transform.position = new Vector3(BG.transform.position.x, worldGenerator.settings[0].heightAddition + worldGenerator.settings[0].heightMultiplier * 0.6f, BG.transform.position.z);

        cam = Camera.main.transform;
        previousCamPos = cam.position;

        // ������ı���ͼ�㶼��BG���Ӷ���
        List<Transform> backgroundList = new List<Transform>();
        foreach (Transform child in BG.transform)
        {
            if (child.name != Constants.Name.SUN && child.name != Constants.Name.MOON && child.name != Constants.Name.RED_MOON)
            {
                backgroundList.Add(child);
            }
        }

        backgrounds = backgroundList.ToArray();
        parallaxScales = new float[backgrounds.Length];
        for (int i = 0; i < backgrounds.Length; i++)
        {
            parallaxScales[i] = backgrounds[i].position.z * -1;
        }
    }

    private void Start()
    {
        GameEvents.current.OnDayStarted += OnDayStarted;
        GameEvents.current.OnNightStarted += OnNightStarted;
    }

    private void Update()
    {
        Vector3 camMoveDelta = cam.position - previousCamPos;


        for (int i = 0; i < backgrounds.Length; i++)
        {
            float parallax = (previousCamPos.y - cam.position.y) * parallaxScales[i];
            float backgroundTargetPosY = backgrounds[i].position.y + parallax;
            Vector3 backgroundTargetPos = new Vector3(backgrounds[i].position.x, backgroundTargetPosY, backgrounds[i].position.z);
            backgrounds[i].position = Vector3.Lerp(backgrounds[i].position, backgroundTargetPos, smoothing * Time.deltaTime);
        }

        for (int i = 0; i < backgrounds.Length; i++)
        {
            float parallaxX = camMoveDelta.x * parallaxScales[i] * scrollSpeed;
            backgrounds[i].position += new Vector3(parallaxX, 0, 0);

            // **检查是否超出视野，进行重置**
            if (cam.position.x - backgrounds[i].position.x >= backgroundWidth)
            {
                backgrounds[i].position += new Vector3(backgroundWidth * backgrounds.Length, 0, 0);
            }
        }

        previousCamPos = cam.position;
    }

    private void OnDestroy()
    {
        GameEvents.current.OnDayStarted -= OnDayStarted;
        GameEvents.current.OnNightStarted -= OnNightStarted;
    }

    private void OnDayStarted()
    {
        sun.SetActive(true);
        redMoon.SetActive(false);
        moon.SetActive(false);
    }

    private void OnNightStarted(bool isRedMoonNight)
    {
        sun.SetActive(false);
        if (isRedMoonNight)
        {
            redMoon.SetActive(true);
            moon.SetActive(false);
        }
        else
        {
            moon.SetActive(true);
            redMoon.SetActive(false);
        }
    }
}