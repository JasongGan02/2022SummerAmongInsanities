using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Medicine", menuName = "Objects/Medicine Object")]
public class medicineObject : BaseObject, IInventoryObject
{
    [SerializeField] private float HealAmount;
    [SerializeField] private float HealTimeCost;
    protected bool isHealing;
    private float healTimeCount;

    [SerializeField]
    private int _maxStack;

    /**
     * implementation of IInventoryObject
     */
    #region
    public int MaxStack
    {
        get => _maxStack;
        set => _maxStack = value;
    }

    public Sprite GetSpriteForInventory()
    {
        return prefab.GetComponent<SpriteRenderer>().sprite;
    }
    public GameObject GetDroppedGameObject(int amount)
    {
        GameObject drop = Instantiate(prefab);
        drop.layer = Constants.Layer.RESOURCE;
        if (drop.GetComponent<Rigidbody2D>() == null)
        {
            drop.AddComponent<Rigidbody2D>();
        }
        drop.transform.localScale = new Vector2(sizeRatio, sizeRatio);
        var controller = drop.AddComponent<DroppedObjectController>();
        controller.Initialize(this, amount);
        drop.transform.localScale = new Vector2(sizeRatio, sizeRatio);

        return drop;
    }
    #endregion

    GameObject player;


    // Start is called before the first frame update
    void Start()
    {
        isHealing = false;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("running");


        if (player == null)
        {
            Debug.Log("player is null");
            try { player = GameObject.FindGameObjectWithTag("Player"); }
            catch { Debug.Log("Player is dead or missing"); }
        }
        else { Debug.Log("found player");  }

        if (Input.GetKey(KeyCode.F) && player != null && !isHealing)
        {
            isHealing = true;
            healTimeCount = HealTimeCost;
        }

        if (isHealing)
        {
            Debug.Log("is Healing");
            healTimeCount -= Time.deltaTime;
            player.GetComponent<PlayerController>().Heal(HealAmount);
            if (healTimeCount <= 0f)
            {
                isHealing = false;
            }
        }
    }
}
