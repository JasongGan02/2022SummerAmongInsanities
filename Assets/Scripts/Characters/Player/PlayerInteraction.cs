using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerInteraction : MonoBehaviour, IAudioable
{
    public float interactRange = 1f;
    public float pickUpRange = 1.5f;
    public LayerMask resourceLayer;
    public LayerMask groundLayer;
    private Animator animator;
    public bool weaponAnim = true;
    private bool isWeaponInUse = false;


    [Header("hold to interact setting")]
    public float waitTime = 1.0f;
    public GameObject[] raycastStartingPoints;
    private float timeStamp = float.MaxValue;
    private GameObject targetObject;
    private PlayerMovement playerMovement;
    private PlayerController playerController;
    private ConstructionModeManager constructionModeManager;
    private Inventory inventory;
    private RectTransform hotbarFirstRow;

    private ShadowGenerator shadowGenerator;

    [Header("tile placement")]
    TerrainGeneration terrainGeneration;
    public int placeTileRange = 15;

    [Header("use item")]
    [SerializeField] private WeaponObject hand;
    private GameObject emptyHand;
    private GameObject gameObjectInUse;
    private WeaponObject currentWeapon;
    private GameObject currentInUseItemIconUI;
    public GameObject equipmentTemplate;
    private InventorySlot currentSlotInUse = null;
    private int indexInUse = EMPTY;
    private float usableObjectPressTime = 0f; // Tracks the time the button has been held


    public float handFarm = 0.5f;
    public float handFrequency = 1;


    private AudioEmitter _audioEmitter;

    private ChestController currentChest;


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
        playerMovement = GetComponent<PlayerMovement>();
        inventory = FindObjectOfType<Inventory>();
        shadowGenerator = FindObjectOfType<ShadowGenerator>();
        terrainGeneration = FindObjectOfType<TerrainGeneration>();
        currentInUseItemIconUI = GameObject.Find(Constants.Name.CURRENT_ITEM_UI);
        _audioEmitter = GetComponent<AudioEmitter>();
    }

    void Start()
    {
        constructionModeManager = FindObjectOfType<ConstructionModeManager>();
        hotbarFirstRow = GameObject.Find("InventoryUI").transform.Find("Hotbar").Find("Row(Clone)").GetComponent<RectTransform>();
        emptyHand = hand.GetSpawnedGameObject(GetComponent<PlayerController>());
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
        if (IsMouseOverInventoryUI()) return;

        PickUpItemCheck();
        HandleUsableObjectUsage();

        if (IsChestOpen())
        {
            currentChest.OpenChest();
        }

        if (!PlayerStatusRepository.GetIsViewingUi())
        {
            BreakTileCheck();
        }

        PlaceTileCheck();
        PlaceTileCancelCheck();
        playAnim();
        HandleQKeyForWeapon();
    }


    private void playAnim()
    {
        if (GetCurrentInUseItem() == null)
        {
            if (Input.GetMouseButton(0))
            {
                animator.SetBool("Hand", true);
            }
            else
            {
                animator.SetBool("Hand", false);
            }
        }
        else if (GetCurrentInUseItem().GetItemName() == "Shovel")
        {
            if (Input.GetMouseButton(0))
            {
                animator.SetBool("Shovel", true);
            }
            else
            {
                animator.SetBool("Shovel", false);
            }
        }
    }


    private bool IsMouseOverInventoryUI()
    {
        Vector2 mousePosition = Input.mousePosition;
        bool isOverUI = RectTransformUtility.RectangleContainsScreenPoint(hotbarFirstRow, mousePosition);
        return isOverUI;
    }

    private bool IsChestOpen()
    {
        bool IsChestOpen = (Input.GetKeyDown(KeyCode.E) && currentChest != null && ConstructionModeManager.IsInConstructionMode == false);
        return IsChestOpen;
    }

    private void HandleSlotLeftClickEvent(object sender, InventoryEventBus.OnSlotLeftClickedEventArgs args)
    {
        UseItemInSlot(args.slotIndex);
    }

    private void HandleUsableObjectUsage()
    {
        if (currentSlotInUse != null && currentSlotInUse.item is UsableObject usableObject)
        {
            // If left mouse button is held
            if (Input.GetMouseButton(0))
            {
                usableObjectPressTime += Time.deltaTime;

                if (usableObjectPressTime >= usableObject.pressDuration)
                {
                    // Trigger the use effect and reset timer
                    usableObject.UseEffect(GetComponent<CharacterController>());
                    usableObjectPressTime = 0f;

                    // Optional: consume the item after use
                    inventory.RemoveItemByOne(indexInUse);
                    if (currentSlotInUse.count <= 0) ClearCurrentItemInUse();
                }
            }
            else
            {
                // Reset the press time if button is released
                usableObjectPressTime = 0f;
            }
        }
    }

    private void UseItemInSlot(int slotIndex)
    {
        if (indexInUse == slotIndex)
        {
            ClearCurrentItemInUse();
            return;
        } //Double Click To Cancel

        ClearCurrentItemInUse();
        indexInUse = slotIndex;
        currentSlotInUse = inventory.GetInventorySlotAtIndex(indexInUse);
        UpdateCurrentInUseItemUI();
        InstantiateCurrentItemInUseGameObject();
    }

    private void InstantiateCurrentItemInUseGameObject()
    {
        if (currentSlotInUse.item is IShadowObject)
        {
            gameObjectInUse = (currentSlotInUse.item as IShadowObject).GetShadowGameObject();
            Debug.Log(gameObjectInUse);
        }

        if (currentSlotInUse.item is WeaponObject)
        {
            Debug.Log("Current item is a WeaponObject.");

            playerController = GetComponent<PlayerController>();
            gameObjectInUse = (currentSlotInUse.item as WeaponObject).GetSpawnedGameObject(playerController);
            currentWeapon = currentSlotInUse.item as WeaponObject;
            waitTime = 1 / currentWeapon?.getfrequency() ?? 1;

            isWeaponInUse = true;
        }
        else
        {
            isWeaponInUse = false;
            waitTime = 1;
        }
    }

    private void HandleQKeyForWeapon()
    {
        if (isWeaponInUse && (Input.GetKeyDown(KeyCode.Q)))
        {
            var weaponInventory = FindObjectOfType<WeaponInventory>(true);

            int targetSlot = -1;
            for (int i = 0; i < weaponInventory.Database.GetSize(); i++)
            {
                var slot = weaponInventory.GetInventorySlotAtIndex(i);
                Debug.Log($"Slot {i}: {(slot?.item != null ? slot.item.GetType().Name : "empty")}");
                if (slot?.item == null)
                {
                    targetSlot = i;
                    break;
                }
            }

            if (targetSlot == -1)
            {
                Debug.LogWarning("No empty slot found in WeaponInventory.");
            }
            else
            {
                inventory.SwapItemsBetweenInventory(weaponInventory, indexInUse, targetSlot);
                ClearCurrentItemInUse();
            }

            isWeaponInUse = false;
        }
    }

    public IInventoryObject GetCurrentInUseItem()
    {
        return currentSlotInUse?.item;
    }

    private void UpdateCurrentInUseItemUI()
    {
        currentInUseItemIconUI.SetActive(true);
        currentInUseItemIconUI.GetComponent<Image>().sprite = currentSlotInUse.item?.GetSpriteForInventory();
    }

    private void ClearCurrentItemInUse()
    {
        if (gameObjectInUse != null)
        {
            Destroy(gameObjectInUse);
            gameObjectInUse = null;
        }

        indexInUse = EMPTY;
        currentSlotInUse = null;
        currentWeapon = null;
        isWeaponInUse = false;
        currentInUseItemIconUI.SetActive(false);
    }

    #region

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
        if (Input.GetMouseButton(0) &&
            (currentSlotInUse == null || (!(currentSlotInUse.item is TowerObject) && !(currentSlotInUse.item is TileObject) && (currentSlotInUse.item.GetItemName() == "Shovel"))))
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
                            _audioEmitter.ChangeLoop("TileDuringBreaking", true);
                            _audioEmitter.PlayClipFromCategory("TileDuringBreaking");
                            targetObject = tempTargetObject;
                            playerMovement.excavateCoeff = 0.1f;
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
            _audioEmitter.ChangeLoop("TileDuringBreaking", false);
            _audioEmitter.StopAudio();
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
        playerMovement.excavateCoeff = 1f;

        ResetTimer();
    }

    private void ClickOnGameObject(GameObject target)
    {
        if (target == null) return;

        IDamageSource damageSource = null;
        if (gameObjectInUse != null)
            damageSource = gameObjectInUse.GetComponent<IDamageSource>();
        else
        {
            damageSource = emptyHand.GetComponent<IDamageSource>();
        }

        if (damageSource == null)
            Debug.LogError("no damage source");

        BreakableObjectController breakableTile = target.GetComponent<BreakableObjectController>();
        if (breakableTile != null)
        {
            breakableTile.SetBreaker(gameObject);
            if (damageSource != null) damageSource.ApplyDamage(breakableTile);
        }
    }

    #endregion

    private void PickUpItemCheck()
    {
        var hit = Physics2D.CircleCast(transform.position, pickUpRange, Vector2.zero, 0, resourceLayer);
        if (hit.transform == null || hit.transform.gameObject.GetComponent<PickupController>() == null) return;
        var droppedObject = hit.transform.gameObject.GetComponent<PickupController>();
        if (!(droppedObject.timeSinceSpawn >= droppedObject.pickupDelay)) return;
        droppedObject.StartTrackingPlayer();
    }

    /***
    Place Tile Implementations

    ***/
    private void PlaceTileCancelCheck()
    {
        if (currentSlotInUse == null ||
            currentSlotInUse.count == 0 ||
            Input.GetMouseButtonDown(1) ||
            Input.GetMouseButtonDown(2) || (!UIViewStateManager.IsViewingConstruction() && UIViewStateManager.IsViewingUI()))
        {
            ClearCurrentItemInUse();
        }
    }

    private void PlaceTileCheck()
    {
        ShadowObjectController shadowObjectController = gameObjectInUse?.GetComponent<ShadowObjectController>();
        if (shadowObjectController != null && currentSlotInUse != null)
        {
            TileGhostPlacementResult result = shadowObjectController.GetTileGhostPlacementResult(currentSlotInUse.item as BaseObject);
            shadowObjectController.transform.position = result.transform.position;
            if (CanPlaceTile(result))
            {
                shadowObjectController.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
                if (Input.GetMouseButtonDown(0))
                {
                    PlaceTile(result.transform);
                }
            }
            else
            {
                shadowObjectController.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.6f, 0.6f, 0.5f);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Chest"))
        {
            currentChest = other.gameObject.GetComponent<ChestController>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Chest") && currentChest == other.gameObject.GetComponent<ChestController>())
        {
            currentChest = null;
        }
    }

    // TODO: inventory should have a map of CollectibleObject -> list of indices
    // and pass this entry to this PlayerInteraction class,
    // so that when the current slot is running out, it can automatically use the next available slot
    private void PlaceTile(Transform transform)
    {
        if (currentSlotInUse.item is TileObject)
        {
            TileObject curTile = (TileObject)currentSlotInUse.item;
            Vector2Int worldPostion = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
            Vector2Int chunkCoord = new Vector2Int(WorldGenerator.GetChunkCoordsFromPosition(worldPostion), 0);
            WorldGenerator.WorldData[chunkCoord][Mathf.FloorToInt(worldPostion.x - chunkCoord.x * WorldGenerator.ChunkSize.x), Mathf.FloorToInt(worldPostion.y), curTile.TileLayer] = curTile;
            WorldGenerator.PlaceTile((TileObject)currentSlotInUse.item, worldPostion.x, worldPostion.y, chunkCoord, false, true);
            WorldGenerator.Instance.RefreshChunkLight(chunkCoord, true);
            inventory.RemoveItemByOne(indexInUse);
        }
        else if (currentSlotInUse.item is TowerObject)
        {
            GameObject newTower = PoolManager.Instance.Get(currentSlotInUse.item as BaseObject);
            newTower.transform.position = transform.position;
            newTower.transform.rotation = transform.rotation;
            constructionModeManager.ConsumeEnergy((((TowerStats)(currentSlotInUse.item as TowerObject)?.baseStats)!).energyCost);
            inventory.RemoveItemByOne(indexInUse);
        }
        else if (currentSlotInUse.item is ChestObject)
        {
            GameObject newTower = PoolManager.Instance.Get(currentSlotInUse.item as BaseObject);
            newTower.transform.position = transform.position;
            newTower.transform.rotation = transform.rotation;
            inventory.RemoveItemByOne(indexInUse);
        }
    }

    private bool CanPlaceTile(TileGhostPlacementResult result)
    {
        return result.canPlaceTile && Vector2.Distance(transform.position, result.transform.position) < placeTileRange;
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


    private const int EMPTY = -1;


    public AudioEmitter GetAudioEmitter()
    {
        return _audioEmitter;
    }
}


public class TileGhostPlacementResult
{
    public Transform transform;
    public bool canPlaceTile;

    public TileGhostPlacementResult(Transform transform, bool canPlaceTile)
    {
        this.transform = transform;
        this.canPlaceTile = canPlaceTile;
    }
}