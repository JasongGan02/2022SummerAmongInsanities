using UnityEngine;

public class ChestController : MonoBehaviour
{
    public InventoryDatabase ChestInventory;

    private UIViewStateManager uiViewStateManager;
    private BaseInventory baseInventory; // Reference to BaseInventory
    private AudioEmitter _audioEmitter;

    void Start()
    {
        ChestInventory = new InventoryDatabase(defaultNumberOfRow: 2, maxExtraRow: 0);
        uiViewStateManager = FindObjectOfType<UIViewStateManager>();
        baseInventory = FindObjectOfType<BaseInventory>(); // Find the BaseInventory instance
        _audioEmitter = GetComponent<AudioEmitter>();
    }

    public void OpenChest()
    {
        baseInventory.SetCurrentInventory(ChestInventory); // Update the BaseInventory
        uiViewStateManager.DisplayChestUI();
        _audioEmitter.PlayClipFromCategory("OpenChest");
    }
}
