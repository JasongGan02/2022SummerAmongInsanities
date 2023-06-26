using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUIManager : MonoBehaviour
{
    private Dictionary<Button, BaseObject> itemButtonToBaseObjectMapping = new Dictionary<Button, BaseObject>();
    private BaseObject selectedBaseObject = null;

    private UIViewStateManager uiViewStateManager;
    private Inventory inventory;

    private GameObject CraftUI;
    private Button CraftButton;
    private Button item0;
    private Image item0Image;
    private Button item1;
    private Image item1Image;
    private Button item2;
    private Image item2Image;
    private Button item3;
    private Image item3Image;
    private Button item4;
    private Image item4Image;

    [SerializeField]
    private BaseObject object0;
    [SerializeField]
    private BaseObject object1;
    [SerializeField]
    private BaseObject object2;
    [SerializeField]
    private BaseObject object3;
    [SerializeField]
    private BaseObject object4;


    private Image outputItem;
    private Image inputItem0;
    private Image inputItem1;
    private Image inputItem2;
    private TMP_Text num0;
    private TMP_Text num1;
    private TMP_Text num2;



    void Start()
    {

        inventory = FindObjectOfType<Inventory>();
        uiViewStateManager = FindObjectOfType<UIViewStateManager>();
        uiViewStateManager.UpdateUiBeingViewedEvent += ToggleCraftUi;
        SetupUI();
    }

    private void OnDestroy()
    {
        uiViewStateManager.UpdateUiBeingViewedEvent -= ToggleCraftUi;
    }

    private void SetupUI()
    {
        CraftUI = GameObject.Find(NAME_CRAFT_UI);
        CraftButton = CraftUI.transform.Find(NAME_Craft_BUTTON).GetComponent<Button>();
        item0 = CraftUI.transform.Find(NAME_0_BUTTON).GetComponent<Button>();
        item0Image = item0.transform.Find("Image").GetComponent<Image>();
        item1 = CraftUI.transform.Find(NAME_1_BUTTON).GetComponent<Button>();
        item1Image = item1.transform.Find("Image").GetComponent<Image>();
        item2 = CraftUI.transform.Find(NAME_2_BUTTON).GetComponent<Button>();
        item2Image = item2.transform.Find("Image").GetComponent<Image>();
        item3 = CraftUI.transform.Find(NAME_3_BUTTON).GetComponent<Button>();
        item3Image = item3.transform.Find("Image").GetComponent<Image>();
        item4 = CraftUI.transform.Find(NAME_4_BUTTON).GetComponent<Button>();
        item4Image = item4.transform.Find("Image").GetComponent<Image>();

        outputItem = CraftUI.transform.Find(NAME_OUTPUT_IMAGE).GetComponent<Image>();
        inputItem0 = CraftUI.transform.Find(NAME_IMAGE_0).GetComponent<Image>();
        inputItem1 = CraftUI.transform.Find(NAME_IMAGE_1).GetComponent<Image>();
        inputItem2 = CraftUI.transform.Find(NAME_IMAGE_2).GetComponent<Image>();

        num0 = CraftUI.transform.Find(NAME_NUM_0).GetComponent<TMP_Text>();
        num1 = CraftUI.transform.Find(NAME_NUM_1).GetComponent<TMP_Text>();
        num2 = CraftUI.transform.Find(NAME_NUM_2).GetComponent<TMP_Text>();

        CraftButton.onClick.AddListener(CraftButtonClicked);
        item0.onClick.AddListener(() => ItemButtonClicked(item0Image.sprite, item0));
        item1.onClick.AddListener(() => ItemButtonClicked(item1Image.sprite, item1));
        item2.onClick.AddListener(() => ItemButtonClicked(item2Image.sprite, item2));
        item3.onClick.AddListener(() => ItemButtonClicked(item3Image.sprite, item3));
        item4.onClick.AddListener(() => ItemButtonClicked(item4Image.sprite, item4));

        itemButtonToBaseObjectMapping.Add(item0, object0);
        itemButtonToBaseObjectMapping.Add(item1, object1);
        itemButtonToBaseObjectMapping.Add(item2, object2);
        itemButtonToBaseObjectMapping.Add(item3, object3);
        itemButtonToBaseObjectMapping.Add(item4, object4);




        //set them to not be visible
        CraftButton.gameObject.SetActive(false);

        outputItem.color = new Color(inputItem0.color.r, inputItem0.color.g, inputItem0.color.b, 0);
        inputItem0.color = new Color(inputItem0.color.r, inputItem0.color.g, inputItem0.color.b, 0);
        inputItem1.color = new Color(inputItem1.color.r, inputItem1.color.g, inputItem1.color.b, 0);
        inputItem2.color = new Color(inputItem2.color.r, inputItem2.color.g, inputItem2.color.b, 0);

        num0.color = new Color(num0.color.r, num0.color.g, num0.color.b, 0);
        num1.color = new Color(num1.color.r, num1.color.g, num1.color.b, 0);
        num2.color = new Color(num2.color.r, num2.color.g, num2.color.b, 0);

        CraftUI.SetActive(false);
 
    }


    private void ToggleCraftUi(object sender, UIBeingViewed ui)
    {
        Debug.Log("ToggleCraftUi called. UIBeingViewed is: " + ui);
        CraftUI.SetActive(ui == UIBeingViewed.Craft);
    }

    

    private void CraftButtonClicked()
    {
        
        if (selectedBaseObject is ICraftableObject craftableObject)
        {
  
            craftableObject.Craft(inventory);
        }

    }

    private void ItemButtonClicked(Sprite itemSprite,Button n)
    {
        outputItem.sprite = itemSprite;
        outputItem.color = new Color(inputItem0.color.r, inputItem0.color.g, inputItem0.color.b, 1);
        inputItem0.color = new Color(inputItem0.color.r, inputItem0.color.g, inputItem0.color.b, 1);
        inputItem1.color = new Color(inputItem1.color.r, inputItem1.color.g, inputItem1.color.b, 1);
        inputItem2.color = new Color(inputItem2.color.r, inputItem2.color.g, inputItem2.color.b, 1);

        num0.color = new Color(num0.color.r, num0.color.g, num0.color.b, 1);
        num1.color = new Color(num1.color.r, num1.color.g, num1.color.b, 1);
        num2.color = new Color(num2.color.r, num2.color.g, num2.color.b, 1);
    
        CraftButton.gameObject.SetActive(true);

        selectedBaseObject = itemButtonToBaseObjectMapping[n];


    }


    private const string NAME_CRAFT_UI = "CraftUI";
    private const string NAME_Craft_BUTTON = "CraftButton";
    private const string NAME_0_BUTTON = "item0";
    private const string NAME_1_BUTTON = "item1";
    private const string NAME_2_BUTTON = "item2";
    private const string NAME_3_BUTTON = "item3";
    private const string NAME_4_BUTTON = "item4";

    private const string NAME_OUTPUT_IMAGE = "OutputItem";
    private const string NAME_IMAGE_0 = "InputItem0";
    private const string NAME_IMAGE_1 = "InputItem1";
    private const string NAME_IMAGE_2 = "InputItem2";
    private const string NAME_NUM_0 = "num0";
    private const string NAME_NUM_1 = "num1";
    private const string NAME_NUM_2 = "num2";







}
