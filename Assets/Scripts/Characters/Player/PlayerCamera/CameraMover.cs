using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class CameraMover : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int zoomOutPPU = 16;
    [SerializeField] private float transitionDuration = 0.5f;

    [Header("Zoom Settings")]
    [SerializeField] private int minPPU = 32; // 最小视野（数值越小，视野越大）
    [SerializeField] private int maxPPU = 128; // 最大视野（数值越大，视野越小）
    [SerializeField] private int zoomStep = 4; // 每次滚轮滚动调整的数值

    [Header("References")]
    [SerializeField] private CameraFollow cameraFollow;
    private PixelPerfectCamera pixelPerfectCamera;

    public bool IsInTransition => isInTransition;

    private bool isInTransition;
    private int preConstructionPPU;


    private void Start()
    {
        if (Camera.main != null) pixelPerfectCamera = Camera.main.GetComponent<PixelPerfectCamera>();
    }

    private void Update()
    {
        if (isInTransition) return; // 过渡期间禁用缩放

        float scroll = -Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            int newPPU = pixelPerfectCamera.assetsPPU - (int)(scroll * zoomStep);
            newPPU = Mathf.Clamp(newPPU, minPPU, maxPPU);
            pixelPerfectCamera.assetsPPU = newPPU;
        }
    }

    public void FocusOnCore(Vector3 corePosition)
    {
        if (isInTransition) return;
        isInTransition = true;
        preConstructionPPU = pixelPerfectCamera.assetsPPU;

        cameraFollow.SetFollowing(false);
        StartCoroutine(TransitionCamera(
            targetPosition: corePosition,
            targetPPU: zoomOutPPU
        ));
    }

    public void ResetToPlayer()
    {
        if (isInTransition) return;
        isInTransition = true;
        // 使用 CameraFollow 计算出的目标位置作为重置目标
        Vector3 targetPosition = cameraFollow.GetPlayerTargetPosition();

        StartCoroutine(TransitionCamera(
            targetPosition: targetPosition,
            targetPPU: preConstructionPPU,
            onComplete: () => cameraFollow.SetFollowing(true)
        ));
    }

    private IEnumerator TransitionCamera(Vector3 targetPosition, int targetPPU, System.Action onComplete = null)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        int startPPU = pixelPerfectCamera.assetsPPU;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / transitionDuration;

            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            pixelPerfectCamera.assetsPPU = Mathf.RoundToInt(Mathf.Lerp(startPPU, targetPPU, t));

            yield return null;
        }

        transform.position = targetPosition;
        pixelPerfectCamera.assetsPPU = targetPPU;
        isInTransition = false;

        onComplete?.Invoke();
    }
}