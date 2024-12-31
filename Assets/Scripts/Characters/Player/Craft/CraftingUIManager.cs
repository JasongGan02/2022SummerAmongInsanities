using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Color = UnityEngine.Color;
using System.Drawing;
using System.Security.Cryptography;

public class CraftingUIManager : MonoBehaviour
{
    private Dictionary<Button, BaseObject> itemButtonToBaseObjectMapping = new Dictionary<Button, BaseObject>();
    private BaseObject selectedBaseObject = null;

    private UIViewStateManager uiViewStateManager;
    private Inventory inventory;
    private CoreArchitectureController coreArchitecture;

    private GameObject MenuUI;

    private Button weapons;
    private Button towers;
    private Button projectiles;
    private Button chests;

    

    public GameObject buttonPrefab;
    public RectTransform content;



    private GameObject CraftUI;
    [SerializeField]
    private ScrollView scrollView;

    private Button CraftButton;

    private Image outputItem;
    private Image inputItem0;
    private Image inputItem1;
    private Image inputItem2;
    private Image inputItem3;
    private Image inputItem4;
    private Image inputItem5;
    private TMP_Text num0;
    private TMP_Text num1;
    private TMP_Text num2;
    private TMP_Text num3;
    private TMP_Text num4;
    private TMP_Text num5;
    private TMP_Text time;

    private BaseObject[] weaponObjects;
    private BaseObject[] towerObjects;
    private BaseObject[] projectileObjects;
    private BaseObject[] chestObjects;
    private BaseObject[] usableObjects;




    private Button TabCraftButton;
    private Button TabStatsButton;
    private GameObject StatsUI;



    [SerializeField] private Sprite craftImage1; 
    [SerializeField] private Sprite craftImage2; 
    [SerializeField] private Sprite statsImage1;
    [SerializeField] private Sprite statsImage2; 


    void Awake()
    {
        weaponObjects = LoadAssets<WeaponObject>("t:WeaponObject");
        towerObjects = LoadAssets<TowerObject>("t:TowerObject");
        chestObjects = LoadAssets<ChestObject>("t:ChestObject");
        usableObjects = LoadAssets<ChestObject>("t:UsableObject");
        Debug.Log(usableObjects.Length);
    }
    
    private T[] LoadAssets<T>(string filter) where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets(filter);
        T[] objects = new T[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            objects[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }
        return objects;
    }

    void Start()
    {
        coreArchitecture = FindObjectOfType<CoreArchitectureController>();
        inventory = FindObjectOfType<Inventory>();
        uiViewStateManager = FindObjectOfType<UIViewStateManager>();
        uiViewStateManager.UpdateUiBeingViewedEvent += ToggleCraftUi;
        SetupMenuUI();
    }


    public BaseObject[] RemoveEmptyRecipeItems(BaseObject[] objects)
    {
        if (objects != null)
        {
            int filteredCount = 0;
            BaseObject[] filteredList = new BaseObject[objects.Length];

            foreach (BaseObject obj in objects)
            {
                ICraftableObject craftableObject = obj as ICraftableObject;
                if (craftableObject != null && craftableObject.getRecipe() != null && craftableObject.getRecipe().Length > 0)
                {
                    filteredList[filteredCount] = obj;
                    filteredCount++;
                }
            }

            BaseObject[] result = new BaseObject[filteredCount];
            Array.Copy(filteredList, result, filteredCount);
            return result;
        }
        else
        {
            
            Debug.LogError("weaponObjects is null or not initialized.");
            return objects;
        }


    }
    

    private void OnDestroy()
    {
        uiViewStateManager.UpdateUiBeingViewedEvent -= ToggleCraftUi;
    }



    private void SetupMenuUI()
    {
        CraftUI = GameObject.Find(NAME_CRAFT_UI);
        MenuUI = GameObject.Find(NAME_CRAFT_MENU);
        StatsUI = GameObject.Find(NAME_STATS_UI);

        weapons = MenuUI.transform.Find(NAME_BUTTON_0).GetComponent<Button>();
        towers = MenuUI.transform.Find(NAME_BUTTON_1).GetComponent<Button>();
        projectiles = MenuUI.transform.Find(NAME_BUTTON_2).GetComponent<Button>();
        chests = MenuUI.transform.Find(NAME_BUTTON_3).GetComponent<Button>();

        TabCraftButton = GameObject.Find(NAME_TAB_CRAFT).GetComponent<Button>();
        TabStatsButton = GameObject.Find(NAME_TAB_STATS).GetComponent<Button>();


        TabCraftButton.GetComponent<Image>().sprite = craftImage1;
        TabStatsButton.GetComponent<Image>().sprite = statsImage2;


        CraftButton = CraftUI.transform.Find(NAME_Craft_BUTTON).GetComponent<Button>();
        CraftButton.onClick.AddListener(CraftButtonClicked);
        CraftButton.gameObject.SetActive(false);

        weapons.onClick.AddListener(() => SetupCraftUI(RemoveEmptyRecipeItems(weaponObjects)));
        towers.onClick.AddListener(() => SetupCraftUI(RemoveEmptyRecipeItems(towerObjects)));
        projectiles.onClick.AddListener(() => SetupCraftUI(RemoveEmptyRecipeItems(usableObjects)));
        chests.onClick.AddListener(() => SetupCraftUI(RemoveEmptyRecipeItems(chestObjects)));
        TabCraftButton.onClick.AddListener(ShowCraftUI);
        TabStatsButton.onClick.AddListener(ShowStatsUI);

        CraftUI.SetActive(false);
        StatsUI.SetActive(false);
        TabCraftButton.gameObject.SetActive(false);
        TabStatsButton.gameObject.SetActive(false);

    }

    


    private void SetupCraftUI(BaseObject[] list)
    {
        CraftUI.SetActive(true);
      
        
        outputItem = CraftUI.transform.Find(NAME_OUTPUT_IMAGE).GetComponent<Image>();
        inputItem0 = CraftUI.transform.Find(NAME_IMAGE_0).GetComponent<Image>();
        inputItem1 = CraftUI.transform.Find(NAME_IMAGE_1).GetComponent<Image>();
        inputItem2 = CraftUI.transform.Find(NAME_IMAGE_2).GetComponent<Image>();

        inputItem3 = CraftUI.transform.Find(NAME_IMAGE_3).GetComponent<Image>();
        inputItem4 = CraftUI.transform.Find(NAME_IMAGE_4).GetComponent<Image>();
        inputItem5 = CraftUI.transform.Find(NAME_IMAGE_5).GetComponent<Image>();

        num0 = CraftUI.transform.Find(NAME_NUM_0).GetComponent<TMP_Text>();
        num1 = CraftUI.transform.Find(NAME_NUM_1).GetComponent<TMP_Text>();
        num2 = CraftUI.transform.Find(NAME_NUM_2).GetComponent<TMP_Text>();
        num3 = CraftUI.transform.Find(NAME_NUM_3).GetComponent<TMP_Text>();
        num4 = CraftUI.transform.Find(NAME_NUM_4).GetComponent<TMP_Text>();
        num5 = CraftUI.transform.Find(NAME_NUM_5).GetComponent<TMP_Text>();
        time = CraftUI.transform.Find("time").GetComponent<TMP_Text>();

        
        

        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);  
        }
        for (int i = 0; i < list.Length; i++)
        {
            
            GameObject buttonObj = Instantiate(buttonPrefab, content);
            Button button = buttonObj.GetComponent<Button>();   
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
           
            buttonRect.sizeDelta = new Vector2(106, 106);
            buttonRect.anchoredPosition = new Vector2(0, -i * 112 + 255);

            itemButtonToBaseObjectMapping[button] = list[i];
         
            GameObject buttonImage = new GameObject();
            buttonImage.transform.SetParent(buttonObj.transform);

            Image image = buttonImage.AddComponent<Image>();
            RectTransform ImageRect = buttonImage.GetComponent<RectTransform>();
            ImageRect.localPosition = new Vector2(0, 0);
            ImageRect.sizeDelta = new Vector2(78, 78);



            image.sprite = list[i].getPrefabSprite();

            CoreArchitectureController coreArchitecture = FindObjectOfType<CoreArchitectureController>();

            ICraftableObject isCraftable = list[i] as ICraftableObject;
            if (isCraftable.getIsCraftable()) 
            {
                if (isCraftable.getIsCoreNeeded())
                {
                    if (coreArchitecture.IsPlayerInConstructionRange() == false)
                    {
                        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.3f);
                    }
                }
            }
            else
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0.3f);
            }
            

            button.onClick.AddListener(() => ItemButtonClicked(image.sprite, button));

        }



        //set them to not be visible
        

        outputItem.color = new Color(outputItem.color.r, outputItem.color.g, outputItem.color.b, 0);
        inputItem0.color = new Color(inputItem0.color.r, inputItem0.color.g, inputItem0.color.b, 0);
        inputItem1.color = new Color(inputItem1.color.r, inputItem1.color.g, inputItem1.color.b, 0);
        inputItem2.color = new Color(inputItem2.color.r, inputItem2.color.g, inputItem2.color.b, 0);

        inputItem3.color = new Color(inputItem3.color.r, inputItem3.color.g, inputItem3.color.b, 0);
        inputItem4.color = new Color(inputItem4.color.r, inputItem4.color.g, inputItem4.color.b, 0);
        inputItem5.color = new Color(inputItem5.color.r, inputItem5.color.g, inputItem5.color.b, 0);
        time.color = new Color(time.color.r, time.color.g, time.color.b, 0);
        num0.color = new Color(num0.color.r, num0.color.g, num0.color.b, 0);
        num1.color = new Color(num1.color.r, num1.color.g, num1.color.b, 0);
        num2.color = new Color(num2.color.r, num2.color.g, num2.color.b, 0);

        num3.color = new Color(num3.color.r, num3.color.g, num3.color.b, 0);
        num4.color = new Color(num4.color.r, num4.color.g, num4.color.b, 0);
        num5.color = new Color(num5.color.r, num5.color.g, num5.color.b, 0);

    }


    private void ToggleCraftUi(object sender, UIBeingViewed ui)
    {
        Debug.Log("ToggleCraftUi called. UIBeingViewed is: " + ui);
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
        CraftUI.SetActive(true);
        TabCraftButton.gameObject.SetActive(true);
        TabStatsButton.gameObject.SetActive(true);
        PlayerStatusRepository.SetIsViewingUi(true);
        SetupCraftUI(RemoveEmptyRecipeItems(weaponObjects));
        UpdateTabIcons(craftImage1, statsImage2);
    }

    private void CraftUIOff()
    {
        CraftUI.SetActive(false);
        StatsUI.SetActive(false);
        TabCraftButton.gameObject.SetActive(false);
        TabStatsButton.gameObject.SetActive(false);
        PlayerStatusRepository.SetIsViewingUi(false);
    }
    private void UpdateTabIcons(Sprite craftSprite, Sprite statsSprite)
    {
        TabCraftButton.GetComponent<Image>().sprite = craftSprite;
        TabStatsButton.GetComponent<Image>().sprite = statsSprite;
    }

    private void ShowCraftUI()
    {
        CraftUI.SetActive(true);
        StatsUI.SetActive(false);
        UpdateTabIcons(craftImage1, statsImage2);
    }

    private void ShowStatsUI()
    {
        CraftUI.SetActive(false);
        StatsUI.SetActive(true);
        UpdateTabIcons(craftImage2, statsImage1);
    }

    private void CraftButtonClicked()
    {
        
            ICraftableObject craftableObject = selectedBaseObject as ICraftableObject;
            craftableObject.Craft(inventory);
            UpdateUi();
        
    }

    private void ItemButtonClicked(Sprite itemSprite,Button n)
    {
        outputItem.sprite = itemSprite;
        outputItem.color = new Color(outputItem.color.r, outputItem.color.g, outputItem.color.b, 1);
        selectedBaseObject = itemButtonToBaseObjectMapping[n];
        time.text = (selectedBaseObject as ICraftableObject).getCraftTime().ToString() + "'s";
        time.color = new Color(time.color.r, time.color.g, time.color.b, 1);

        CraftButton.gameObject.SetActive(true);

            
        UpdateUi();

    }
        

    private void UpdateUi()
    {
        ICraftableObject craftableObject = selectedBaseObject as ICraftableObject;
        if (coreArchitecture == null)
            coreArchitecture = CoreArchitectureController.Instance;
        if (craftableObject.getIsCraftable())
        {
            if (craftableObject.getIsCoreNeeded())
            {
                if (coreArchitecture.IsPlayerInConstructionRange())
                {
                    enableInputItems();
                    
                }
                else
                {
                    CraftButton.gameObject.SetActive(false);
                    disableInputItems();
                }
            }
            enableInputItems();
        }
        else
        {
            CraftButton.gameObject.SetActive(false);
            disableInputItems();
        }

    }

    private void enableInputItems()
    {
        ICraftableObject craftableObject = selectedBaseObject as ICraftableObject;
        CraftRecipe[] inputItems = craftableObject.getRecipe();

        if (inputItems.Length == 0)
        {
            inputItem0.color = new Color(inputItem0.color.r, inputItem0.color.g, inputItem0.color.b, 0);
            num0.color = new Color(num0.color.r, num0.color.g, num0.color.b, 0);

        }
        else
        {
            inputItem0.sprite = inputItems[0].material.getPrefabSprite();
            inputItem0.color = new Color(inputItem0.color.r, inputItem0.color.g, inputItem0.color.b, 1);
            num0.text = inventory.FindItemCount(inputItems[0].material) + "/" + inputItems[0].quantity;
            num0.color = new Color(num0.color.r, num0.color.g, num0.color.b, 1);
        }

        if (inputItems.Length <= 1)
        {
            inputItem1.color = new Color(inputItem1.color.r, inputItem1.color.g, inputItem1.color.b, 0);
            num1.color = new Color(num1.color.r, num1.color.g, num1.color.b, 0);

        }
        else
        {
            inputItem1.sprite = inputItems[1].material.getPrefabSprite();
            inputItem1.color = new Color(inputItem1.color.r, inputItem1.color.g, inputItem1.color.b, 1);
            num1.text = inventory.FindItemCount(inputItems[1].material) + "/" + inputItems[1].quantity;
            num1.color = new Color(num1.color.r, num1.color.g, num1.color.b, 1);
        }


        if (inputItems.Length <= 2)
        {
            inputItem2.color = new Color(inputItem2.color.r, inputItem2.color.g, inputItem2.color.b, 0);
            num2.color = new Color(num2.color.r, num2.color.g, num2.color.b, 0);
        }
        else
        {
            inputItem2.sprite = inputItems[2].material.getPrefabSprite();
            inputItem2.color = new Color(inputItem2.color.r, inputItem2.color.g, inputItem2.color.b, 1);
            num2.text = inventory.FindItemCount(inputItems[2].material) + "/" + inputItems[2].quantity;
            num2.color = new Color(num2.color.r, num2.color.g, num2.color.b, 1);
        }

        if (inputItems.Length <= 3)
        {
            inputItem3.color = new Color(inputItem3.color.r, inputItem3.color.g, inputItem3.color.b, 0);
            num3.color = new Color(num3.color.r, num3.color.g, num3.color.b, 0);
        }
        else
        {
            inputItem3.sprite = inputItems[3].material.getPrefabSprite();
            inputItem3.color = new Color(inputItem3.color.r, inputItem3.color.g, inputItem3.color.b, 1);
            num3.text = inventory.FindItemCount(inputItems[3].material) + "/" + inputItems[3].quantity;
            num3.color = new Color(num3.color.r, num3.color.g, num3.color.b, 1);
        }

        if (inputItems.Length <= 4)
        {
            inputItem4.color = new Color(inputItem4.color.r, inputItem4.color.g, inputItem4.color.b, 0);
            num4.color = new Color(num4.color.r, num4.color.g, num4.color.b, 0);
        }
        else
        {
            inputItem4.sprite = inputItems[4].material.getPrefabSprite();
            inputItem4.color = new Color(inputItem4.color.r, inputItem4.color.g, inputItem4.color.b, 1);
            num4.text = inventory.FindItemCount(inputItems[4].material) + "/" + inputItems[4].quantity;
            num4.color = new Color(num4.color.r, num4.color.g, num4.color.b, 1);
        }


        if (inputItems.Length <= 5)
        {
            inputItem5.color = new Color(inputItem5.color.r, inputItem5.color.g, inputItem5.color.b, 0);
            num5.color = new Color(num5.color.r, num5.color.g, num5.color.b, 0);
        }
        else
        {
            inputItem5.sprite = inputItems[5].material.getPrefabSprite();
            inputItem5.color = new Color(inputItem5.color.r, inputItem5.color.g, inputItem5.color.b, 1);
            num5.text = inventory.FindItemCount(inputItems[5].material) + "/" + inputItems[5].quantity;
            num5.color = new Color(num5.color.r, num5.color.g, num5.color.b, 1);
        }


    }
    private void disableInputItems()
    {
        inputItem0.color = new Color(inputItem0.color.r, inputItem0.color.g, inputItem0.color.b, 0);
        num0.color = new Color(num0.color.r, num0.color.g, num0.color.b, 0);
        inputItem1.color = new Color(inputItem1.color.r, inputItem1.color.g, inputItem1.color.b, 0);
        num1.color = new Color(num1.color.r, num1.color.g, num1.color.b, 0);
        inputItem2.color = new Color(inputItem2.color.r, inputItem2.color.g, inputItem2.color.b, 0);
        num2.color = new Color(num2.color.r, num2.color.g, num2.color.b, 0);

        inputItem3.color = new Color(inputItem3.color.r, inputItem3.color.g, inputItem3.color.b, 0);
        num3.color = new Color(num3.color.r, num3.color.g, num3.color.b, 0);
        inputItem4.color = new Color(inputItem4.color.r, inputItem4.color.g, inputItem4.color.b, 0);
        num4.color = new Color(num4.color.r, num4.color.g, num4.color.b, 0);
        inputItem5.color = new Color(inputItem5.color.r, inputItem5.color.g, inputItem5.color.b, 0);
        num5.color = new Color(num5.color.r, num5.color.g, num5.color.b, 0);
    }





    private const string NAME_CRAFT_MENU = "CraftMenu";
    private const string NAME_BUTTON_0 = "type0";
    private const string NAME_BUTTON_1 = "type1";
    private const string NAME_BUTTON_2 = "type2";
    private const string NAME_BUTTON_3 = "type3";



    private const string NAME_CRAFT_UI = "CraftUI";
    private const string NAME_Craft_BUTTON = "CraftButton";


    private const string NAME_OUTPUT_IMAGE = "OutputItem";
    private const string NAME_IMAGE_0 = "InputItem0";
    private const string NAME_IMAGE_1 = "InputItem1";
    private const string NAME_IMAGE_2 = "InputItem2";
    private const string NAME_IMAGE_3 = "InputItem3";
    private const string NAME_IMAGE_4 = "InputItem4";
    private const string NAME_IMAGE_5 = "InputItem5";
    private const string NAME_NUM_0 = "num0";
    private const string NAME_NUM_1 = "num1";
    private const string NAME_NUM_2 = "num2";
    private const string NAME_NUM_3 = "num3";
    private const string NAME_NUM_4 = "num4";
    private const string NAME_NUM_5 = "num5";

    private const string NAME_STATS_UI = "StatsComponet";

    private const string NAME_TAB_CRAFT = "TabCraftButton";
    private const string NAME_TAB_STATS = "TabStatsButton";

    private const string NAME_SCROLLVIEW = "ScrollView";






}