﻿using System.Collections.Generic;
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

    
    private BaseObject[] weaponObjects;
    private BaseObject[] towerObjects;
    private BaseObject[] projectileObjects;
    private BaseObject[] chestObjects;

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



    void Awake()
    {
        //TODO: ProjectileObject

        // 使用 AssetDatabase 来查找所有的 WeaponObject
        string[] weaponguids = AssetDatabase.FindAssets("t:WeaponObject");
        string[] towerguids = AssetDatabase.FindAssets("t:TowerObject");
        string[] projectileguids = AssetDatabase.FindAssets("t:ProjectileObject");
        string[] chestguids = AssetDatabase.FindAssets("t:ChestObject");

        // 根据 GUID 获取对应的路径，并加载 ScriptableObject
        weaponObjects = new WeaponObject[weaponguids.Length];
        towerObjects = new TowerObject[towerguids.Length];
        projectileObjects = new ProjectileObject[projectileguids.Length];
        chestObjects = new ChestObject[chestguids.Length];



        for (int i = 0; i < weaponguids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(weaponguids[i]);
            weaponObjects[i] = AssetDatabase.LoadAssetAtPath<WeaponObject>(path);
        }
        for (int i = 0; i < towerguids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(towerguids[i]);
            towerObjects[i] = AssetDatabase.LoadAssetAtPath<TowerObject>(path);
        }
        for (int i = 0; i < projectileguids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(projectileguids[i]);
            projectileObjects[i] = AssetDatabase.LoadAssetAtPath<ProjectileObject>(path);
        }
        for (int i = 0; i < chestguids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(chestguids[i]);
            chestObjects[i] = AssetDatabase.LoadAssetAtPath<ChestObject>(path);
        }



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
        MenuUI = GameObject.Find(NAME_CRAFT_MENU);
        CraftUI = GameObject.Find(NAME_CRAFT_UI);
        weapons = MenuUI.transform.Find(NAME_BUTTON_0).GetComponent<Button>();
        towers = MenuUI.transform.Find(NAME_BUTTON_1).GetComponent<Button>();
        projectiles = MenuUI.transform.Find(NAME_BUTTON_2).GetComponent<Button>();
        chests = MenuUI.transform.Find(NAME_BUTTON_3).GetComponent<Button>();

        CraftButton = CraftUI.transform.Find(NAME_Craft_BUTTON).GetComponent<Button>();
        CraftButton.onClick.AddListener(CraftButtonClicked);
        CraftButton.gameObject.SetActive(false);

        weapons.onClick.AddListener(() => SetupCraftUI(RemoveEmptyRecipeItems(weaponObjects)));
        towers.onClick.AddListener(() => SetupCraftUI(RemoveEmptyRecipeItems(towerObjects)));
        projectiles.onClick.AddListener(() => SetupCraftUI(RemoveEmptyRecipeItems(projectileObjects)));
        chests.onClick.AddListener(() => SetupCraftUI(RemoveEmptyRecipeItems(chestObjects)));

        MenuUI.SetActive(false);
        CraftUI.SetActive(false);
        
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
           
            buttonRect.sizeDelta = new Vector2(120, 120);
            buttonRect.anchoredPosition = new Vector2(0, -i * 130 + 190);

            itemButtonToBaseObjectMapping[button] = list[i];
         
            GameObject buttonImage = new GameObject();
            buttonImage.transform.SetParent(buttonObj.transform);

            Image image = buttonImage.AddComponent<Image>();
            RectTransform ImageRect = buttonImage.GetComponent<RectTransform>();
            ImageRect.localPosition = new Vector2(0, 0);



            image.sprite = list[i].getPrefabSprite();

            CoreArchitectureController coreArchitecture = FindObjectOfType<CoreArchitectureController>();

            ICraftableObject isCraftable = list[i] as ICraftableObject;
            if (isCraftable.getIsCraftable()) 
            {
                if (isCraftable.getIsCoreNeeded())
                {
                    if (coreArchitecture.IsPlayerInControlRange() == false)
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
            MenuUIOn();
        }
        else
        {
            BothUIOff();
        }
        
    }

    private void MenuUIOn()
    {
        MenuUI.SetActive(true);
        CraftUI.SetActive(true);
        PlayerStatusRepository.SetIsViewingUi(true);
    }

    private void BothUIOff()
    {
        MenuUI.SetActive(false);
        CraftUI.SetActive(false);
        PlayerStatusRepository.SetIsViewingUi(false);
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
                if (coreArchitecture.IsPlayerInControlRange())
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
        BaseObject[] inputItems = craftableObject.getRecipe();
        int[] inputQuantities = craftableObject.getQuantity();

        if (inputItems.Length == 0)
        {
            inputItem0.color = new Color(inputItem0.color.r, inputItem0.color.g, inputItem0.color.b, 0);
            num0.color = new Color(num0.color.r, num0.color.g, num0.color.b, 0);

        }
        else
        {
            inputItem0.sprite = inputItems[0].getPrefabSprite();
            inputItem0.color = new Color(inputItem0.color.r, inputItem0.color.g, inputItem0.color.b, 1);
            num0.text = inventory.FindItemCount(inputItems[0]) + "/" + inputQuantities[0];
            num0.color = new Color(num0.color.r, num0.color.g, num0.color.b, 1);
        }

        if (inputItems.Length <= 1)
        {
            inputItem1.color = new Color(inputItem1.color.r, inputItem1.color.g, inputItem1.color.b, 0);
            num1.color = new Color(num1.color.r, num1.color.g, num1.color.b, 0);

        }
        else
        {
            inputItem1.sprite = inputItems[1].getPrefabSprite();
            inputItem1.color = new Color(inputItem1.color.r, inputItem1.color.g, inputItem1.color.b, 1);
            num1.text = inventory.FindItemCount(inputItems[1]) + "/" + inputQuantities[1];
            num1.color = new Color(num1.color.r, num1.color.g, num1.color.b, 1);
        }


        if (inputItems.Length <= 2)
        {
            inputItem2.color = new Color(inputItem2.color.r, inputItem2.color.g, inputItem2.color.b, 0);
            num2.color = new Color(num2.color.r, num2.color.g, num2.color.b, 0);
        }
        else
        {
            inputItem2.sprite = inputItems[2].getPrefabSprite();
            inputItem2.color = new Color(inputItem2.color.r, inputItem2.color.g, inputItem2.color.b, 1);
            num2.text = inventory.FindItemCount(inputItems[2]) + "/" + inputQuantities[2];
            num2.color = new Color(num2.color.r, num2.color.g, num2.color.b, 1);
        }

        if (inputItems.Length <= 3)
        {
            inputItem3.color = new Color(inputItem3.color.r, inputItem3.color.g, inputItem3.color.b, 0);
            num3.color = new Color(num3.color.r, num3.color.g, num3.color.b, 0);
        }
        else
        {
            inputItem3.sprite = inputItems[3].getPrefabSprite();
            inputItem3.color = new Color(inputItem3.color.r, inputItem3.color.g, inputItem3.color.b, 1);
            num3.text = inventory.FindItemCount(inputItems[3]) + "/" + inputQuantities[3];
            num3.color = new Color(num3.color.r, num3.color.g, num3.color.b, 1);
        }

        if (inputItems.Length <= 4)
        {
            inputItem4.color = new Color(inputItem4.color.r, inputItem4.color.g, inputItem4.color.b, 0);
            num4.color = new Color(num4.color.r, num4.color.g, num4.color.b, 0);
        }
        else
        {
            inputItem4.sprite = inputItems[4].getPrefabSprite();
            inputItem4.color = new Color(inputItem4.color.r, inputItem4.color.g, inputItem4.color.b, 1);
            num4.text = inventory.FindItemCount(inputItems[4]) + "/" + inputQuantities[4];
            num4.color = new Color(num4.color.r, num4.color.g, num4.color.b, 1);
        }


        if (inputItems.Length <= 5)
        {
            inputItem5.color = new Color(inputItem5.color.r, inputItem5.color.g, inputItem5.color.b, 0);
            num5.color = new Color(num5.color.r, num5.color.g, num5.color.b, 0);
        }
        else
        {
            inputItem5.sprite = inputItems[5].getPrefabSprite();
            inputItem5.color = new Color(inputItem5.color.r, inputItem5.color.g, inputItem5.color.b, 1);
            num5.text = inventory.FindItemCount(inputItems[5]) + "/" + inputQuantities[5];
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


    private const string NAME_SCROLLVIEW = "ScrollView";






}
