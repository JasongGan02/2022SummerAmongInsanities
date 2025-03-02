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
    [Tooltip("Label or key used to load from Addressables. For multiple items, use a label.")]
    public string addressOrLabel; // e.g., "t:WeaponObject"
    public Button categoryButton; // The button to click in the UI
    [HideInInspector]
    public BaseObject[] loadedObjects; // Filled at runtime
}

public class CraftingUIManager : MonoBehaviour
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
    [SerializeField] private GameObject CharacterUI;
    [SerializeField] private GameObject craftUI;
    [SerializeField] private GameObject craftMenuUI;
    [SerializeField] private List<GameObject> statsUIList = new List<GameObject>();


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
    [SerializeField] private RectTransform content; // Parent of the scrolling content
    [SerializeField] private ScrollView scrollView; // If using UI Toolkit's ScrollView
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

    [Header("Lock Sprite")]
    [SerializeField] private Sprite lockSprite;

    // -----------------------------------------------------
    //  Monobehaviour: Load asset references, init listeners
    // -----------------------------------------------------
    private async void Start()
    {
        // 1) Find references
        coreArchitecture = FindFirstObjectByType<CoreArchitectureController>();
        inventory = FindFirstObjectByType<Inventory>();
        uiViewStateManager = FindFirstObjectByType<UIViewStateManager>();

        // 2) Load each category’s objects from Addressables
        //    (assuming you used labels or addresses for each category)
        foreach (var cat in categoryConfigs)
        {
            // Log the category being loaded
            //Debug.Log($"Loading category: {cat.addressOrLabel}");

            // Attempt to load assets
            var results = await AddressablesManager.Instance.LoadMultipleAssetsAsync<BaseObject>(cat.addressOrLabel);
            if (results == null || results.Count == 0)
            {
                Debug.LogError($"No objects found for category: {cat.addressOrLabel}");
            }
            else
            {
                //Debug.Log($"Loaded {results.Count} objects for category: {cat.addressOrLabel}");
            }

            cat.loadedObjects = results != null ? new List<BaseObject>(results).ToArray() : new BaseObject[0];
        }

        // 3) Subscribe to UI events
        uiViewStateManager.UpdateUiBeingViewedEvent += ToggleCraftUi;

        // 4) Initialize the UI
        SetupUI();
    }


    private void OnDestroy()
    {
        if (uiViewStateManager != null)
        {
            uiViewStateManager.UpdateUiBeingViewedEvent -= ToggleCraftUi;
        }
    }

    // -------------------------
    //  Top-Level Menu UI Setup
    // -------------------------
    private void SetupUI()
    {
        if (statsUIList != null)
        {
            foreach (var statsUI in statsUIList)
            {
                if (statsUI != null) statsUI.SetActive(false);
            }
        }

        if (craftUI != null) craftUI.SetActive(false);
        //if (CharacterUI != null) CharacterUI.SetActive(false);

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

        // Dictionary to track which buttons already have listeners
        var buttonListeners = new Dictionary<Button, List<BaseObject>>();

        // Now set up each category button in the UI
        foreach (var cat in categoryConfigs)
        {
            if (cat.categoryButton != null)
            {
                //Debug.Log($"Setting up button for category: {cat.addressOrLabel}");

                if (!buttonListeners.ContainsKey(cat.categoryButton))
                {
                    buttonListeners[cat.categoryButton] = new List<BaseObject>();
                    cat.categoryButton.onClick.AddListener(() =>
                    {
                        // Aggregate all items tied to this button
                        var aggregatedItems = buttonListeners[cat.categoryButton].ToArray();
                        //Debug.Log($"Showing {aggregatedItems.Length} items for shared button.");
                        SetupCraftUI(aggregatedItems);
                    });
                }

                // Add the loaded objects to the shared list
                if (cat.loadedObjects != null)
                {
                    buttonListeners[cat.categoryButton].AddRange(RemoveEmptyRecipeItems(cat.loadedObjects));
                }
            }
            else
            {
                Debug.LogError($"No button assigned for category: {cat.addressOrLabel}");
            }
        }
    }


    // --------------------------------------------
    //  Show / Hide entire Craft or Stats UI logic
    // --------------------------------------------
    private void ToggleCraftUi(object sender, UIBeingViewed ui)
    {
        if (ui == UIBeingViewed.Craft)
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
        if (CharacterUI != null) CharacterUI.SetActive(true);
        if (craftUI != null) craftUI.SetActive(true);
        if (statsUIList != null)
        {
            foreach (var statsUI in statsUIList)
            {
                if (statsUI != null) statsUI.SetActive(false);
            }
        }

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
        //if (CharacterUI != null) CharacterUI.SetActive(false);
        if (craftUI != null) craftUI.SetActive(false);
        if (statsUIList != null)
        {
            foreach (var statsUI in statsUIList)
            {
                if (statsUI != null) statsUI.SetActive(false);
            }
        }

        tabCraftButton.gameObject.SetActive(false);
        tabStatsButton.gameObject.SetActive(false);

        PlayerStatusRepository.SetIsViewingUi(false);
    }

    private void ShowCraftUI()
    {
        if (craftUI != null) craftUI.SetActive(true);
        if (statsUIList != null)
        {
            foreach (var statsUI in statsUIList)
            {
                if (statsUI != null) statsUI.SetActive(false);
            }
        }

        UpdateTabIcons(craftImage1, statsImage2);
    }

    private void ShowStatsUI()
    {
        if (craftUI != null) craftUI.SetActive(false);
        if (statsUIList != null)
        {
            foreach (var statsUI in statsUIList)
            {
                if (statsUI != null) statsUI.SetActive(true);
            }
        }

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
            if (craftableObject != null && craftableObject.IsCraftable &&
                craftableObject.GetRecipe() != null &&
                craftableObject.GetRecipe().Length > 0)
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
        if (list == null || list.Length == 0)
        {
            Debug.LogError("No objects to display in the Crafting UI.");
            return;
        }


        Array.Sort(list, (a, b) =>
        {
            ICraftableObject craftA = a as ICraftableObject;
            ICraftableObject craftB = b as ICraftableObject;
            bool aLocked = craftA != null && craftA.GetIsLocked();
            bool bLocked = craftB != null && craftB.GetIsLocked();
            if (aLocked == bLocked)
                return 0; 
            return aLocked ? 1 : -1;
        });


        //Debug.Log($"Setting up UI for {list.Length} objects.");

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
            CoreArchitectureController coreArchitecture = FindObjectOfType<CoreArchitectureController>();
            // Dim if not craftable or out of range
            ICraftableObject craftObj = list[i] as ICraftableObject;
            if (craftObj == null || !craftObj.GetIsCraftable())
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0.3f);
            }
            else if (craftObj.GetIsCoreNeeded() && !coreArchitecture.IsPlayerInConstructionRange())
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0.3f);
            }
            if (craftObj != null && craftObj.GetIsLocked())
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0.5f);
                GameObject lockOverlay = new GameObject("LockOverlay");
                lockOverlay.transform.SetParent(buttonObj.transform, false); 
                Image lockImage = lockOverlay.AddComponent<Image>();
                lockImage.sprite = lockSprite;
            }
 
            buttonComponent.onClick.AddListener(() => OnItemButtonClicked(image.sprite, buttonComponent));
        }

        // Hide item details by default
        HideAllInputItems();
        craftButton.gameObject.SetActive(false);

        // Hide output
        if (outputItem != null)
        {
            outputItem.color = new Color(1, 1, 1, 0);
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
            outputItem.color = new Color(1, 1, 1, 1);
        }

        // Show craft time
        ICraftableObject craftableObject = selectedBaseObject as ICraftableObject;
        if (craftableObject != null && timeText != null)
        {
            timeText.text = craftableObject.GetCraftTime().ToString() + "s";
            timeText.color = new Color(timeText.color.r, timeText.color.g, timeText.color.b, 1);
        }

        craftButton.gameObject.SetActive(true);
        UpdateUI();
    }

    private void CraftButtonClicked()
    {
        if (selectedBaseObject == null)
        {
            Debug.LogError("CraftButtonClicked: selectedBaseObject is null!");
            return;
        }

        ICraftableObject craftableObject = selectedBaseObject as ICraftableObject;
        if (craftableObject == null)
        {
            Debug.LogError("CraftButtonClicked: The selectedBaseObject does not implement ICraftableObject!");
            return;
        }

        craftableObject.Craft(inventory);

        UpdateUI(); // refresh
    }


    // ---------------------
    //  Refresh UI on craft
    // ---------------------
    private void UpdateUI()
    {
        ICraftableObject craftableObject = selectedBaseObject as ICraftableObject;
        if (coreArchitecture == null)
            coreArchitecture = CoreArchitectureController.Instance;
        if (craftableObject == null)
        {
            craftButton.gameObject.SetActive(false);
            HideAllInputItems();
            return;
        }

        // Check if craftable at all
        if (!craftableObject.GetIsCraftable())
        {
            craftButton.gameObject.SetActive(false);
            HideAllInputItems();
            return;
        }

        if (craftableObject.GetIsLocked())
        {
            craftButton.gameObject.SetActive(false);
            HideAllInputItems();
            HideInputItem(outputItem, timeText);
            return;
        }

        // If core needed, check range
        if (craftableObject.GetIsCoreNeeded() && !coreArchitecture.IsPlayerInConstructionRange())
        {
            craftButton.gameObject.SetActive(false);
            ShowInputItems(craftableObject.GetRecipe());
            return;
        }

        // Enable everything
        ShowInputItems(craftableObject.GetRecipe());
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
        if (image != null) image.color = new Color(1, 1, 1, 0);
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
            image.rectTransform.localScale = new Vector3(0.9f, 0.9f, 1f);
            image.color = new Color(1, 1, 1, 1);
        }

        if (text != null)
        {
            int haveCount = inventory.FindItemCount(recipe.material);
            text.text = $"{haveCount}/{recipe.quantity}";
            text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
        }
    }
}