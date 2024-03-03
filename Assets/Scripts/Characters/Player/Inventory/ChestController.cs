using UnityEngine;

public class ChestController : MonoBehaviour
{
    public InventoryDatabase ChestInventory;

    private UIViewStateManager uiViewStateManager;
    private BaseInventory baseInventory; // Reference to BaseInventory

    void Start()
    {
        ChestInventory = new InventoryDatabase(defaultNumberOfRow: 2, maxExtraRow: 0);
        uiViewStateManager = FindObjectOfType<UIViewStateManager>();
        baseInventory = FindObjectOfType<BaseInventory>(); // Find the BaseInventory instance
    }

    public void OpenChest()
    {
        baseInventory.SetCurrentInventory(ChestInventory); // Update the BaseInventory
        uiViewStateManager.DisplayChestUI();
    }
}
