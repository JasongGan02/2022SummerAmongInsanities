using System.Collections.Generic;
using UnityEngine;

public class ConstructionPanelUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject towerEntryPrefab;
    [SerializeField] private TowerDatabase towerDatabase;
    [SerializeField] private Inventory playerInventory; // reference to the player's inventory

    private Dictionary<TowerObject, ConstructionTowerEntry> entries = new Dictionary<TowerObject, ConstructionTowerEntry>();
    private TowerObject selectedTower;

    void Start()
    {
        InitializePanel();
    }

    /// <summary>
    /// Initialize the panel by creating an entry for each tower.
    /// </summary>
    private void InitializePanel()
    {
        foreach (TowerObject tower in towerDatabase.availableTowers)
        {
            TowerEntryState state;
            if (!tower.IsUnlocked)
            {
                state = TowerEntryState.Locked;
            }
            else if (playerInventory.CheckRecipeHasEnoughMaterials(tower.recipe))
            {
                state = TowerEntryState.Buildable;
            }
            else
            {
                state = TowerEntryState.LackMaterials;
            }

            CreateTowerEntry(tower, state);
        }
    }

    /// <summary>
    /// Create a UI entry for the given tower with the specified state.
    /// </summary>
    private void CreateTowerEntry(TowerObject tower, TowerEntryState state)
    {
        GameObject entryObj = Instantiate(towerEntryPrefab, contentParent);
        ConstructionTowerEntry entry = entryObj.GetComponent<ConstructionTowerEntry>();

        entry.Initialize(
            tower.itemName,
            tower.towerIcon,
            state,
            () => OnTowerSelected(tower)
        );
        entries[tower] = entry;
    }

    /// <summary>
    /// Called when a tower is selected. Only buildable towers trigger a placement session.
    /// </summary>
    private void OnTowerSelected(TowerObject tower)
    {
        if (!tower.IsUnlocked || !playerInventory.CheckRecipeHasEnoughMaterials(tower.recipe))
            return;

        selectedTower = tower;
        // Use the new Placement Manager to start the placement session.
        ConstructionPlacementManager.Instance.StartPlacement(tower, (position) => { PlaceTower(selectedTower, position); });
    }

    /// <summary>
    /// Deduct the materials from inventory, instantiate the tower in construction mode,
    /// and attach a construction progress bar.
    /// </summary>
    private void PlaceTower(TowerObject tower, Vector3 position)
    {
        // Deduct required items from inventory
        playerInventory.CraftItems(tower.recipe, tower);

        // Instantiate the tower prefab at the desired position
        GameObject towerGO = PoolManager.Instance.Get(tower);
        towerGO.name = tower.itemName;

        // Attach a construction progress component to handle the build time and progress bar.
        ConstructionProgress progress = towerGO.AddComponent<ConstructionProgress>();
        progress.Initialize(tower.towerStats.constructionTime);
    }
}