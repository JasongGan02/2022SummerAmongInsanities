using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float aheadDistance = 0f;
    [SerializeField] private float smoothSpeed = 720f;
    [SerializeField] private float zOffset = -1f;

    private Transform playerTransform;
    private bool isFollowing = true;

    private void Update()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        if (isFollowing)
        {
            UpdateCameraPosition();
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void UpdateCameraPosition()
    {
        Vector3 targetPosition = GetPlayerTargetPosition();

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.unscaledDeltaTime
        );
    }

    // 公开计算玩家目标位置的方法
    public Vector3 GetPlayerTargetPosition()
    {
        if (playerTransform == null)
        {
            return transform.position;
        }

        Vector3 targetPosition = playerTransform.position + new Vector3(0, 0, zOffset);
        float directionMultiplier = Mathf.Sign(playerTransform.localScale.x);
        targetPosition.x += aheadDistance * directionMultiplier;
        return targetPosition;
    }

    public void SetFollowing(bool enable)
    {
        isFollowing = enable;
        if (enable) UpdateCameraPosition();
    }
}