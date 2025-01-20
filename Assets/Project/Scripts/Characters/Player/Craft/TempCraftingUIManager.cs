using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements; // For ScrollView if needed
using UnityEditor;
using UnityEngine.Serialization;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image; // For AssetDatabase usage

// ----------------------------------
// Helper class to store category data
// ----------------------------------
[Serializable]
public class CategoryConfig
{
    public string categoryName;    // e.g., "Weapons"
    public string assetFilter;     // e.g., "t:WeaponObject"
    public Button categoryButton;  // The button to click in the UI
    [HideInInspector] 
    public BaseObject[] loadedObjects; // Filled at runtime
}

public class TempCraftingUIManager : MonoBehaviour
{
    // -----------------------
    //  Data / Dependencies
    // -----------------------
    [Header("Categories Configuration")]
    [Tooltip("Add a new entry for each category you wish to have in the Craft Menu.")]
    public List<CategoryConfig> categoryConfigs = new List<CategoryConfig>();

    private Dictionary<Button, BaseObject> itemButtonToBaseObjectMapping = new Dictionary<Button, BaseObject>();
    private BaseObject selectedBaseObject = null;

    private UIViewStateManager uiViewStateManager;
    private Inventory inventory;
    private CoreArchitectureController coreArchitecture;

    [FormerlySerializedAs("menuUI")]
    [Header("Core UI")]
    [SerializeField] private GameObject craftUI;
    [SerializeField] private GameObject craftMenuUI;
    [SerializeField] private GameObject statsUI;

    [Header("Tab Buttons")]
    [SerializeField] private Button tabCraftButton;
    [SerializeField] private Button tabStatsButton;

    [Header("Tab Sprites")]
    [SerializeField] private Sprite craftImage1;
    [SerializeField] private Sprite craftImage2;
    [SerializeField] private Sprite statsImage1;
    [SerializeField] private Sprite statsImage2;

    [Header("Craft UI References")]
    [SerializeField] private GameObject buttonPrefab; // Prefab used to list items in the scroll area
    [SerializeField] private RectTransform content;   // Parent of the scrolling content
    [SerializeField] private ScrollView scrollView;   // If using UI Toolkit's ScrollView
    [SerializeField] private Button craftButton;
    [SerializeField] private Image outputItem;
    [SerializeField] private Image inputItem0;
    [SerializeField] private Image inputItem1;
    [SerializeField] private Image inputItem2;
    [SerializeField] private Image inputItem3;
    [SerializeField] private Image inputItem4;
    [SerializeField] private Image inputItem5;
    [SerializeField] private TMP_Text num0;
    [SerializeField] private TMP_Text num1;
    [SerializeField] private TMP_Text num2;
    [SerializeField] private TMP_Text num3;
    [SerializeField] private TMP_Text num4;
    [SerializeField] private TMP_Text num5;
    [SerializeField] private TMP_Text timeText;

    // -----------------------------------------------------
    //  Monobehaviour: Load asset references, init listeners
    // -----------------------------------------------------
    private void Awake()
    {
        // 1) Load each category’s objects from the Editor’s AssetDatabase
        //    (Only works in the Editor or Editor-like environment)
        foreach (var cat in categoryConfigs)
        {
            cat.loadedObjects = LoadAssets<BaseObject>(cat.assetFilter);
        }
    }

    private void Start()
    {
        // 2) Find references to needed controllers
        coreArchitecture = FindObjectOfType<CoreArchitectureController>();
        inventory = FindObjectOfType<Inventory>();
        uiViewStateManager = FindObjectOfType<UIViewStateManager>();

        // 3) Subscribe to UI view state changes
        uiViewStateManager.UpdateUiBeingViewedEvent += ToggleCraftUi;

        // 4) Initialize main menu UI
        SetupMenuUI();
    }

    private void OnDestroy()
    {
        if (uiViewStateManager != null)
        {
            uiViewStateManager.UpdateUiBeingViewedEvent -= ToggleCraftUi;
        }
    }

    // ------------------------------
    // Generic load method by filter
    // ------------------------------
    private T[] LoadAssets<T>(string filter) where T : ScriptableObject
    {
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets(filter);
        T[] objects = new T[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            objects[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }
        return objects;
#else
        // In a runtime build, you'd need a different system.
        return new T[0];
#endif
    }

    // -------------------------
    //  Top-Level Menu UI Setup
    // -------------------------
    private void SetupMenuUI()
    {
        if (craftUI != null) craftUI.SetActive(false);
        if (statsUI != null) statsUI.SetActive(false);

        // Hide tab buttons at first
        tabCraftButton.gameObject.SetActive(false);
        tabStatsButton.gameObject.SetActive(false);

        // Swap icons: default to craft tab
        tabCraftButton.GetComponent<Image>().sprite = craftImage1;
        tabStatsButton.GetComponent<Image>().sprite = statsImage2;

        // Listen to tab button events
        tabCraftButton.onClick.AddListener(ShowCraftUI);
        tabStatsButton.onClick.AddListener(ShowStatsUI);

        // Listen to craft button
        craftButton.onClick.AddListener(CraftButtonClicked);
        craftButton.gameObject.SetActive(false);

        // Now set up each category button in the UI
        // By hooking them to call SetupCraftUI
        foreach (var cat in categoryConfigs)
        {
            if (cat.categoryButton != null)
            {
                // Local reference capture
                var localCat = cat;
                cat.categoryButton.onClick.AddListener(() => 
                {
                    // Filter out objects with empty recipes, then pass them in
                    var filtered = RemoveEmptyRecipeItems(localCat.loadedObjects);
                    SetupCraftUI(filtered);
                });
            }
        }
    }

    // --------------------------------------------
    //  Show / Hide entire Craft or Stats UI logic
    // --------------------------------------------
    private void ToggleCraftUi(object sender, UIBeingViewed ui)
    {
        if(ui == UIBeingViewed.Craft)
        {
            CraftUIOn();
        }
        else
        {
            CraftUIOff();
        }
    }

    private void CraftUIOn()
    {
        if (craftUI != null) craftUI.SetActive(true);
        tabCraftButton.gameObject.SetActive(true);
        tabStatsButton.gameObject.SetActive(true);

        PlayerStatusRepository.SetIsViewingUi(true);

        // By default, show the first category’s items (if available)
        if (categoryConfigs.Count > 0)
        {
            var first = RemoveEmptyRecipeItems(categoryConfigs[0].loadedObjects);
            SetupCraftUI(first);
        }

        UpdateTabIcons(craftImage1, statsImage2);
    }

    private void CraftUIOff()
    {
        if (craftUI != null) craftUI.SetActive(false);
        if (statsUI != null) statsUI.SetActive(false);
        tabCraftButton.gameObject.SetActive(false);
        tabStatsButton.gameObject.SetActive(false);

        PlayerStatusRepository.SetIsViewingUi(false);
    }

    private void ShowCraftUI()
    {
        if (craftUI != null) craftUI.SetActive(true);
        if (statsUI != null) statsUI.SetActive(false);
        UpdateTabIcons(craftImage1, statsImage2);
    }

    private void ShowStatsUI()
    {
        if (craftUI != null) craftUI.SetActive(false);
        if (statsUI != null) statsUI.SetActive(true);
        UpdateTabIcons(craftImage2, statsImage1);
    }

    private void UpdateTabIcons(Sprite craftSprite, Sprite statsSprite)
    {
        tabCraftButton.GetComponent<Image>().sprite = craftSprite;
        tabStatsButton.GetComponent<Image>().sprite = statsSprite;
    }

    // ------------------------
    //  Filter out empty recipe
    // ------------------------
    public BaseObject[] RemoveEmptyRecipeItems(BaseObject[] objects)
    {
        if (objects == null) return null;

        List<BaseObject> filtered = new List<BaseObject>();
        foreach (BaseObject obj in objects)
        {
            ICraftableObject craftableObject = obj as ICraftableObject;
            if (craftableObject != null &&
                craftableObject.getRecipe() != null &&
                craftableObject.getRecipe().Length > 0)
            {
                filtered.Add(obj);
            }
        }
        return filtered.ToArray();
    }

    // ---------------------------------------
    //  Builds the item list for a given set
    // ---------------------------------------
    private void SetupCraftUI(BaseObject[] list)
    {
        if (craftUI == null) return;
        craftUI.SetActive(true);

        // Clear out old item list in the scroll content
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }

        // Build new item buttons
        for (int i = 0; i < list.Length; i++)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, content);
            Button buttonComponent = buttonObj.GetComponent<Button>();

            // Layout / positioning
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(106, 106);
            buttonRect.anchoredPosition = new Vector2(0, -i * 112 + 255);

            // Keep mapping so we know which object is associated
            itemButtonToBaseObjectMapping[buttonComponent] = list[i];

            // Create & set an image inside the button
            GameObject buttonImage = new GameObject("ItemImage");
            buttonImage.transform.SetParent(buttonObj.transform);

            Image image = buttonImage.AddComponent<Image>();
            RectTransform imageRect = buttonImage.GetComponent<RectTransform>();
            imageRect.localPosition = Vector2.zero;
            imageRect.sizeDelta = new Vector2(78, 78);

            // Set sprite
            image.sprite = list[i].getPrefabSprite();

            // Dim if not craftable or out of range
            ICraftableObject craftObj = list[i] as ICraftableObject;
            if (craftObj == null || !craftObj.getIsCraftable())
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0.3f);
            }
            else if (craftObj.getIsCoreNeeded() && !coreArchitecture.IsPlayerInConstructionRange())
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0.3f);
            }

            // Wire up the click
            buttonComponent.onClick.AddListener(() => OnItemButtonClicked(image.sprite, buttonComponent));
        }

        // Hide item details by default
        HideAllInputItems();
        craftButton.gameObject.SetActive(false);

        // Hide output
        if (outputItem != null)
        {
            outputItem.color = new Color(1,1,1,0);
        }
        if (timeText != null)
        {
            timeText.color = new Color(timeText.color.r, timeText.color.g, timeText.color.b, 0);
        }
    }

    // ----------------------------
    //  Handle item selection click
    // ----------------------------
    private void OnItemButtonClicked(Sprite itemSprite, Button clickedButton)
    {
        selectedBaseObject = itemButtonToBaseObjectMapping[clickedButton];

        // Show the output item
        if (outputItem != null && itemSprite != null)
        {
            outputItem.sprite = itemSprite;
            outputItem.color = new Color(1,1,1,1);
        }

        // Show craft time
        ICraftableObject craftableObject = selectedBaseObject as ICraftableObject;
        if (craftableObject != null && timeText != null)
        {
            timeText.text = craftableObject.getCraftTime().ToString() + "s";
            timeText.color = new Color(timeText.color.r, timeText.color.g, timeText.color.b, 1);
        }

        craftButton.gameObject.SetActive(true);
        UpdateUI();
    }

    private void CraftButtonClicked()
    {
        if (selectedBaseObject == null) return;

        // Attempt craft
        ICraftableObject craftableObject = selectedBaseObject as ICraftableObject;
        if (craftableObject != null)
        {
            craftableObject.Craft(inventory);
            UpdateUI(); // refresh
        }
    }

    // ---------------------
    //  Refresh UI on craft
    // ---------------------
    private void UpdateUI()
    {
        ICraftableObject craftableObject = selectedBaseObject as ICraftableObject;
        if (craftableObject == null)
        {
            craftButton.gameObject.SetActive(false);
            HideAllInputItems();
            return;
        }

        // Check if craftable at all
        if (!craftableObject.getIsCraftable())
        {
            craftButton.gameObject.SetActive(false);
            HideAllInputItems();
            return;
        }

        // If core needed, check range
        if (craftableObject.getIsCoreNeeded() && !coreArchitecture.IsPlayerInConstructionRange())
        {
            craftButton.gameObject.SetActive(false);
            HideAllInputItems();
            return;
        }

        // Enable everything
        ShowInputItems(craftableObject.getRecipe());
    }

    private void ShowInputItems(CraftRecipe[] inputItems)
    {
        // For up to 6 inputs, show them
        // Always check array length first
        SetInputItem(inputItem0, num0, inputItems, 0);
        SetInputItem(inputItem1, num1, inputItems, 1);
        SetInputItem(inputItem2, num2, inputItems, 2);
        SetInputItem(inputItem3, num3, inputItems, 3);
        SetInputItem(inputItem4, num4, inputItems, 4);
        SetInputItem(inputItem5, num5, inputItems, 5);
    }

    private void HideAllInputItems()
    {
        HideInputItem(inputItem0, num0);
        HideInputItem(inputItem1, num1);
        HideInputItem(inputItem2, num2);
        HideInputItem(inputItem3, num3);
        HideInputItem(inputItem4, num4);
        HideInputItem(inputItem5, num5);
    }

    private void HideInputItem(Image image, TMP_Text text)
    {
        if (image != null) image.color = new Color(1,1,1,0);
        if (text != null) text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
    }

    private void SetInputItem(Image image, TMP_Text text, CraftRecipe[] items, int index)
    {
        if (index >= items.Length)
        {
            HideInputItem(image, text);
            return;
        }

        var recipe = items[index];
        if (image != null)
        {
            image.sprite = recipe.material.getPrefabSprite();
            image.color = new Color(1,1,1,1);
        }

        if (text != null)
        {
            int haveCount = inventory.FindItemCount(recipe.material);
            text.text = $"{haveCount}/{recipe.quantity}";
            text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
        }
    }
}
