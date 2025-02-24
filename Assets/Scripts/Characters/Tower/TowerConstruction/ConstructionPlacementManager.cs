using UnityEngine;
using System;

public class ConstructionPlacementManager : MonoBehaviour
{
    public static ConstructionPlacementManager Instance { get; private set; }

    private TowerObject currentTower;
    private GameObject currentShadow;
    private ShadowObjectController currentShadowController;
    private Action<Vector3> onPlacementConfirmed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Begin a placement session with the given shadow prefab.
    /// When placement is confirmed, the callback receives the final (adjusted) position.
    /// </summary>
    public void StartPlacement(TowerObject towerObject, Action<Vector3> placementCallback)
    {
        ClearPlacement();
        currentTower = towerObject;
        currentShadow = currentTower.GetShadowGameObject();
        currentShadowController = currentShadow.GetComponent<ShadowObjectController>();
        onPlacementConfirmed = placementCallback;
    }

    private void Update()
    {
        if (currentShadow != null)
        {
            UpdateShadowPosition();
            CheckPlacementInput();
            CheckCancelInput();
        }
    }

    /// <summary>
    /// Update the shadow's position by snapping to grid and positioning one grid cell above the ground.
    /// </summary>
    private void UpdateShadowPosition()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentShadow.transform.position = GetAdjustedSnappedCoordinate(mousePos);
    }

    /// <summary>
    /// Snap the mouse position to the grid and adjust the Y coordinate to be one cell above the first ground object.
    /// </summary>
    private Vector2 GetAdjustedSnappedCoordinate(Vector2 mousePos)
    {
        // First, snap to grid (assuming grid cells of 1 unit with centers at 0.5 offset)
        Vector2 snapped = new Vector2(Mathf.Floor(mousePos.x) + 0.5f, Mathf.Floor(mousePos.y) + 0.5f);

        // Now, cast a ray from above the snapped position downwards to find the ground.
        // We start a few units above to guarantee a hit.
        Vector2 origin = new Vector2(snapped.x, snapped.y + 5f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 10f, 1 << Constants.Layer.GROUND);

        if (hit.collider != null)
        {
            // Calculate the ground's center position (assuming ground tiles are centered at .5 offset)
            float groundCenterY = Mathf.Floor(hit.point.y) + 0.5f;
            // Position one grid cell above the ground tile.
            snapped.y = groundCenterY + 1f;
        }

        return snapped;
    }

    private void CheckPlacementInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (currentShadowController != null && currentShadowController.IsValidPlacement(currentTower))
            {
                onPlacementConfirmed?.Invoke(currentShadow.transform.position);
                ClearPlacement();
            }
        }
    }

    private void CheckCancelInput()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            ClearPlacement();
        }
    }

    public void ClearPlacement()
    {
        if (currentShadow != null)
        {
            currentTower = null;
            Destroy(currentShadow);
            currentShadow = null;
            currentShadowController = null;
            onPlacementConfirmed = null;
        }
    }
}