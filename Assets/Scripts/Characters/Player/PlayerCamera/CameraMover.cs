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

    // 不再缓存原始位置，而是在重置时动态计算
    private bool isInTransition;

    private void Start()
    {
        pixelPerfectCamera = Camera.main.GetComponent<PixelPerfectCamera>();
    }

    private void Update()
    {
        // 监听鼠标滚轮输入，并调整视野大小
        float scroll = -Input.mouseScrollDelta.y;
        if (scroll != 0 && !isInTransition)
        {
            int newPPU = pixelPerfectCamera.assetsPPU - (int)(scroll * zoomStep);
            newPPU = Mathf.Clamp(newPPU, minPPU, maxPPU);
            pixelPerfectCamera.assetsPPU = newPPU;
        }
    }

    public void FocusOnCore(Vector3 corePosition)
    {
        if (isInTransition) return;

        cameraFollow.SetFollowing(false);
        StartCoroutine(TransitionCamera(
            targetPosition: corePosition,
            targetPPU: zoomOutPPU
        ));
    }

    public void ResetToPlayer()
    {
        if (isInTransition) return;

        // 使用 CameraFollow 计算出的目标位置作为重置目标
        Vector3 targetPosition = cameraFollow.GetPlayerTargetPosition();
        StartCoroutine(TransitionCamera(
            targetPosition: targetPosition,
            targetPPU: pixelPerfectCamera.assetsPPU, // 或者你希望的其他数值
            onComplete: () => cameraFollow.SetFollowing(true)
        ));
    }

    private IEnumerator TransitionCamera(Vector3 targetPosition, int targetPPU, System.Action onComplete = null)
    {
        isInTransition = true;
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