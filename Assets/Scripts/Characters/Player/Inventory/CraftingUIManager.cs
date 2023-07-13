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

public class CraftingUIManager : MonoBehaviour
{
    private Dictionary<Button, BaseObject> itemButtonToBaseObjectMapping = new Dictionary<Button, BaseObject>();
    private BaseObject selectedBaseObject = null;

    private UIViewStateManager uiViewStateManager;
    private Inventory inventory;

    private GameObject MenuUI;

    private Button weapons;
    private Button towers;

    
    private BaseObject[] weaponObjects;
    private BaseObject[] towerObjects;
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
    private TMP_Text num0;
    private TMP_Text num1;
    private TMP_Text num2;  


    void Awake()
    {
        // 使用 AssetDatabase 来查找所有的 WeaponObject
        string[] weaponguids = AssetDatabase.FindAssets("t:WeaponObject");
        string[] towerguids = AssetDatabase.FindAssets("t:TowerObject");

        // 根据 GUID 获取对应的路径，并加载 ScriptableObject
        weaponObjects = new WeaponObject[weaponguids.Length];
        towerObjects = new TowerObject[towerguids.Length];

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


    }
    void Start()
    {

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
        weapons.onClick.AddListener(() => SetupCraftUI(RemoveEmptyRecipeItems(weaponObjects)));
        towers.onClick.AddListener(() => SetupCraftUI(RemoveEmptyRecipeItems(towerObjects)));
        MenuUI.SetActive(false);
        CraftUI.SetActive(false);
    }


    private void SetupCraftUI(BaseObject[] list)
    {
        CraftUI.SetActive(true);
        CraftButton = CraftUI.transform.Find(NAME_Craft_BUTTON).GetComponent<Button>();
        
        outputItem = CraftUI.transform.Find(NAME_OUTPUT_IMAGE).GetComponent<Image>();
        inputItem0 = CraftUI.transform.Find(NAME_IMAGE_0).GetComponent<Image>();
        inputItem1 = CraftUI.transform.Find(NAME_IMAGE_1).GetComponent<Image>();
        inputItem2 = CraftUI.transform.Find(NAME_IMAGE_2).GetComponent<Image>();
        num0 = CraftUI.transform.Find(NAME_NUM_0).GetComponent<TMP_Text>();
        num1 = CraftUI.transform.Find(NAME_NUM_1).GetComponent<TMP_Text>();
        num2 = CraftUI.transform.Find(NAME_NUM_2).GetComponent<TMP_Text>();
        CraftButton.onClick.AddListener(CraftButtonClicked);

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

            button.onClick.AddListener(() => ItemButtonClicked(image.sprite, button));

        }



        //set them to not be visible
        CraftButton.gameObject.SetActive(false);

        outputItem.color = new Color(inputItem0.color.r, inputItem0.color.g, inputItem0.color.b, 0);
        inputItem0.color = new Color(inputItem0.color.r, inputItem0.color.g, inputItem0.color.b, 0);
        inputItem1.color = new Color(inputItem1.color.r, inputItem1.color.g, inputItem1.color.b, 0);
        inputItem2.color = new Color(inputItem2.color.r, inputItem2.color.g, inputItem2.color.b, 0);

        num0.color = new Color(num0.color.r, num0.color.g, num0.color.b, 0);
        num1.color = new Color(num1.color.r, num1.color.g, num1.color.b, 0);
        num2.color = new Color(num2.color.r, num2.color.g, num2.color.b, 0);

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
    }

    private void BothUIOff()
    {
        MenuUI.SetActive(false);
        CraftUI.SetActive(false);
    }
    
    

    private void CraftButtonClicked()
    {
        
        if (selectedBaseObject is ICraftableObject craftableObject)
        {
  
            craftableObject.Craft(inventory);
            UpdateUi();
        }


    }

        private void ItemButtonClicked(Sprite itemSprite,Button n)
        {
            outputItem.sprite = itemSprite;
            outputItem.color = new Color(inputItem0.color.r, inputItem0.color.g, inputItem0.color.b, 1);
    
            CraftButton.gameObject.SetActive(true);

            selectedBaseObject = itemButtonToBaseObjectMapping[n];
            UpdateUi();

        }


    private void UpdateUi()
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
            num0.text = inventory.findItemCount(inputItems[0]) + "/" + inputQuantities[0];
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
            num1.text = inventory.findItemCount(inputItems[1]) + "/" + inputQuantities[1];
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
            num2.text = inventory.findItemCount(inputItems[2]) + "/" + inputQuantities[2];
            num2.color = new Color(num2.color.r, num2.color.g, num2.color.b, 1);
        }
    }







    private const string NAME_CRAFT_MENU = "CraftMenu";
    private const string NAME_BUTTON_0 = "type0";
    private const string NAME_BUTTON_1 = "type1";



    private const string NAME_CRAFT_UI = "CraftUI";
    private const string NAME_Craft_BUTTON = "CraftButton";


    private const string NAME_OUTPUT_IMAGE = "OutputItem";
    private const string NAME_IMAGE_0 = "InputItem0";
    private const string NAME_IMAGE_1 = "InputItem1";
    private const string NAME_IMAGE_2 = "InputItem2";
    private const string NAME_NUM_0 = "num0";
    private const string NAME_NUM_1 = "num1";
    private const string NAME_NUM_2 = "num2";


    private const string NAME_SCROLLVIEW = "ScrollView";






}
