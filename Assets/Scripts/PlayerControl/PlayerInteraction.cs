using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 1f;
    public float pickUpRange = 1f;
    public LayerMask resourceLayer;
    public LayerMask groundLayer;
    private Animator animator;

    [Header("hold to interact setting")]
    public float waitTime = 1.0f;
    public GameObject[] raycastStartingPoints;
    private float timeStamp = float.MaxValue;
    private GameObject targetObject;
    private Rigidbody2D rb;
    private Playermovement pm;
    private Inventory inventory;

    private ShadowGenerator shadowGenerator;

    [Header("tile placement")]
    private InventorySlot currentSlotToBeUsed = null;
    private GameObject currentTileGhost = null;
    private int currentIndex = -1;
    public int placeTileRange = 1;


    private Dictionary<Vector2Int, GameObject> _worldTilesDictionary = null;
    private Dictionary<Vector2Int, GameObject> worldTilesDictionary
    {
        get
        {
            if (_worldTilesDictionary == null)
            {
                _worldTilesDictionary = TerrainGeneration.worldTilesDictionary;
            }
            return _worldTilesDictionary;
        }
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        pm = GetComponent<Playermovement>();
        inventory = GetComponent<Inventory>();
        shadowGenerator = FindObjectOfType<ShadowGenerator>();
    }

    private void OnEnable()
    {
        inventory.AddSlotLeftClickedHandler(HandleSlotLeftClickEvent);
    }

    private void OnDisable()
    {
        inventory.RemoveSlotLeftClickedHandler(HandleSlotLeftClickEvent);
    }

    // Update is called once per frame
    void Update()
    {
        PickUpItemCheck();
        if (!PlayerStatusRepository.GetIsViewingUi())
        {
            BreakTileCheck();
        }

        PlaceTileCheck();
        PlaceTileCancelCheck();
    }

    private void HandleSlotLeftClickEvent(object sender, InventoryEventBus.OnSlotLeftClickedEventArgs args)
    {
        InventorySlot slot = inventory.GetInventorySlotAtIndex(args.slotIndex);
        currentIndex = args.slotIndex;
        Debug.Log("PlayerInteraction: using " + slot.item.name);
        switch (slot.item.itemType)
        {
            case ItemType.Material:
                PrepareForPlaceTile(slot);
                break;
        }
    }

    private void PrepareForPlaceTile(InventorySlot slot)
    {
        currentSlotToBeUsed = slot;

        if (currentTileGhost != null)
        {
            Destroy(currentTileGhost);
        } 
        currentTileGhost = Instantiate(currentSlotToBeUsed.item.droppedItem);
        currentTileGhost.GetComponent<DroppedObjectController>().enabled = false;
    }

    private void StartTimer()
    {
        timeStamp = Time.time + waitTime;
    }

    private void ResetTimer()
    {
        timeStamp = Mathf.Infinity;
    }

    private bool IsTimerCompleted()
    {
        return Time.time >= timeStamp;
    }

    private void BreakTileCheck()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mouseDownPosition = GetMousePosition2D();
            if (Vector2.Distance(mouseDownPosition, transform.position) <= interactRange)
            {
                RaycastHit2D clickHit = Physics2D.Raycast(mouseDownPosition, Vector2.zero);
                if (clickHit.transform != null)
                {
                    GameObject tempTargetObject = clickHit.transform.gameObject;
                    if (tempTargetObject.GetComponent<BreakableObjectController>() != null && CanInteractWith(tempTargetObject, mouseDownPosition))
                    {
                        animator.SetBool(Constants.Animator.MELEE_TOOL, true);
                        if (clickHit.transform.gameObject != targetObject)
                        {
                            targetObject = tempTargetObject;
                            Debug.Log("local: " + targetObject.transform.localPosition);
                            Debug.Log("global: " + targetObject.transform.position);
                            pm.excavateCoeff = 0.1f;
                            StartTimer();
                        }
                    }
                    else
                    {
                        
                        ResetMeleeAnimationAndTimer();
                    }
                }
                else
                {
                    
                    ResetMeleeAnimationAndTimer();
                }
            } 
            else
            {
                
                ResetMeleeAnimationAndTimer();
            }

            
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            ResetMeleeAnimationAndTimer();
        }

        if (IsTimerCompleted())
        {
            if (targetObject != null)
            {
                ClickOnGameObject(targetObject);
            }
            
            StartTimer();
        }
    }

    private void ResetMeleeAnimationAndTimer()
    {
        targetObject = null;
        animator.SetBool(Constants.Animator.MELEE_TOOL, false);
        pm.excavateCoeff=1f;

        ResetTimer();
    }

    private void PickUpItemCheck()
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, pickUpRange, Vector2.zero, 0, resourceLayer);
        if (hit.transform != null)
        {
            DroppedObjectController resoureObject = hit.transform.gameObject.GetComponent<DroppedObjectController>();
            if (inventory.CanAddItem(resoureObject.collectibleObject))
            {
                resoureObject.PickingUp();
            }
        }
    }

    private void PlaceTileCancelCheck()
    {
        if (currentSlotToBeUsed == null ||
            currentSlotToBeUsed.count == 0 ||
            Input.GetMouseButtonDown(1) || 
            Input.GetMouseButtonDown(2) || 
            UIViewStateManager.isViewingUI())
        {
            currentSlotToBeUsed = null;
            if (currentTileGhost != null)
            {
                Destroy(currentTileGhost);
                currentTileGhost = null;
            }
            currentIndex = -1;
        }
    }

    private void PlaceTileCheck()
    {
        if (currentSlotToBeUsed != null)
        {
            TileGhostPlacementResult result = GetTileGhostPlacementResult();
            currentTileGhost.transform.position = result.position;
            if (CanPlaceTile(result))
            {
                currentTileGhost.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
                if (Input.GetMouseButtonDown(0))
                {
                    PlaceTile(result.position);
                }
            }
            else
            {
                currentTileGhost.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
            }

            
        }
    }

    // TODO: inventory should have a map of CollectibleObject -> list of indices
    // and pass this entry to this PlayerInteraction class,
    // so that when the current slot is running out, it can automatically use the next available slot
    private void PlaceTile(Vector2 position)
    {
        GameObject newTile = Instantiate(currentSlotToBeUsed.item.terrainTile);
        newTile.transform.position = position;
        newTile.transform.localScale = new Vector2(0.25f, 0.25f);
        inventory.RemoveItemByOne(currentIndex);
    }

    private bool CanPlaceTile(TileGhostPlacementResult result)
    {
        return result.canPlaceTile && Vector2.Distance(transform.position, result.position) < placeTileRange;
    }

    private TileGhostPlacementResult GetTileGhostPlacementResult()
    {
        Vector2 mousePosition = GetMousePosition2D();
        float x = GetSnappedCoordinate(mousePosition.x);
        float y = GetSnappedCoordinate(mousePosition.y);

        if (worldTilesDictionary.ContainsKey(new Vector2Int((int)(x * 4), (int)(y * 4)))) {
            return new TileGhostPlacementResult(new Vector2(x, y), false);
        } 
        else
        {
            return new TileGhostPlacementResult(new Vector2(x, y), true);
        }
    }

    private float GetSnappedCoordinate(float number)
    {
        switch (number % 1)
        {
            case float res when (res >= 0.25 && res < 0.5):
                return (int)number + 0.375f;
                
            case float res when (res >= 0.5 && res < 0.75):
                return (int)number + 0.625f;
                
            case float res when (res >= 0.75 && res < 1.0):
                return (int)number + 0.875f;
                
            case float res when (res >= 0.0 && res < 0.25):
                return (int)number + 0.125f;
            default:
                return -1; // impossible to get here
        }
    }

    private Vector2 GetMousePosition2D()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(mousePos.x, mousePos.y);
    }

    private bool CanInteractWith(GameObject tempTargetObject, Vector2 mouseDownPosition)
    {
        if (tempTargetObject == null) return false;

        if (tempTargetObject.layer == Constants.Layer.GROUND)
        {
            foreach (GameObject raycastStartingPoint in raycastStartingPoints)
            {
                Vector2 direction = mouseDownPosition - new Vector2(raycastStartingPoint.transform.position.x, raycastStartingPoint.transform.position.y);
                RaycastHit2D hit = Physics2D.Raycast(raycastStartingPoint.transform.position, direction, interactRange, groundLayer);
                if (hit.transform != null && hit.transform.gameObject == tempTargetObject)
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            return !tempTargetObject.CompareTag(Constants.Tag.RESOURCE);
        }
            
    }

    private void ClickOnGameObject(GameObject target)
    {
        if (target == null) return;

        Vector2Int coord = new Vector2Int((int)target.transform.localPosition.x, (int)target.transform.localPosition.y);
        shadowGenerator.OnTileBroken(coord);
        BreakableObjectController breakableTile = target.GetComponent<BreakableObjectController>();
        if (breakableTile != null)
        {
            breakableTile.OnClicked();
        }
        else
        {
            Destroy(target);
        }
    }
}

class TileGhostPlacementResult
{
    public Vector2 position;
    public bool canPlaceTile;

    public TileGhostPlacementResult(Vector2 position, bool canPlaceTile)
    {
        this.position = position;
        this.canPlaceTile = canPlaceTile;
    }
}
