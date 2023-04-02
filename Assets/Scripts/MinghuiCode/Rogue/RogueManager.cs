using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RogueManager : MonoBehaviour
{
    [SerializeField]
    private RogueGraph graph;    

    [SerializeField]
    private GameObject buffSelectionTemplate;

    private UIViewStateManager uiViewStateManager;
    private GameObject rogueUI;
    private Button levelUpButton;
    private TMP_Text selectedBuffText;
    private GameObject buffContainer;
    private Inventory inventory;

    public List<RogueGraphNode> selectedNodes = new();

    public List<RogueGraphNode> DebugList = new();

    //PlayerExp Control
    [SerializeField] private int levelUpCost;
    
    // Start is called before the first frame update
    void Start()
    {
        uiViewStateManager = FindObjectOfType<UIViewStateManager>();
        uiViewStateManager.UpdateUiBeingViewedEvent += ToggleRogueUI;
        inventory = FindObjectOfType<Inventory>();

        selectedNodes.Add(graph.rootNode);
        SetupUI();
    }

    private void OnDestroy()
    {
        uiViewStateManager.UpdateUiBeingViewedEvent -= ToggleRogueUI;
    }

    private void SetupUI()
    {
        rogueUI = GameObject.Find(NAME_ROGUE_UI);
        levelUpButton = rogueUI.transform.Find(NAME_LEVEL_UP_BUTTON).GetComponent<Button>();
        selectedBuffText = rogueUI.transform.Find(NAME_SELECTED_BUFF_TEXT).GetComponent<TMP_Text>();
        buffContainer = rogueUI.transform.Find(NAME_BUFF_CONTAINER).gameObject;

        levelUpButton.onClick.AddListener(OnLevelUpButtonClicked);

        rogueUI.SetActive(false);
    }

    private void OnLevelUpButtonClicked()
    {
        
        if (buffContainer.transform.childCount == 0)
        {
            if(inventory.spendEXP(levelUpCost))
            {
                AddBuffs();
            }
            
        }
        
    }

    private void AddBuffs()
    {
        List<RogueGraphNode> nodes = GetRandomBuffNodes();

        for (int i = 0; i < nodes.Count; i++)
        {
            RogueGraphNode node = nodes[i];
            GameObject buffCard = Instantiate(buffSelectionTemplate);
            BuffSelectionController buffSelectionController = buffCard.GetComponent<BuffSelectionController>();
            buffSelectionController.Init(node, buffContainer.transform, new Vector2(460 + 500 * i, 590f));
            buffSelectionController.OnBuffSelectedEvent += HandleBuffSelectedEvent;
        }
    }

    private List<RogueGraphNode> GetRandomBuffNodes()
    {
        List<RogueGraphNode> candidateNodes = new();

        foreach (RogueGraphNode selectedNode in selectedNodes)
        {
            foreach (RogueGraphNode node in selectedNode.childNodes)
            {
                if (CanAddNodeToCandidates(node, candidateNodes))
                {
                    candidateNodes.Add(node);
                }
            }
        }

        DebugList = candidateNodes;

        if (candidateNodes.Count <= 3)
        {
            return candidateNodes;
        } 
        else
        {
            HashSet<int> indice = new();
            while(indice.Count < 3)
            {
                int index = (int)Random.Range(0f, candidateNodes.Count - 0.01f);
                if (indice.Contains(index)) continue;
                indice.Add(index);
            }

            List<RogueGraphNode> sublist = new();
            foreach (int index in indice)
            {
                sublist.Add(candidateNodes[index]);
            }

            return sublist;
        }
    }

    private bool CanAddNodeToCandidates(RogueGraphNode node, List<RogueGraphNode> candidateNodes)
    {
        if (selectedNodes.Contains(node)) return false;

        if (candidateNodes.Contains(node)) return false;

        if (node.parentNodes.Count <= 1) return true;

        foreach (RogueGraphNode parentNode in node.parentNodes)
        {
            if (!selectedNodes.Contains(parentNode)) return false;
        }

        return true;
    }

    private void ToggleRogueUI(object sender, UIBeingViewed ui)
    {
        rogueUI.SetActive(ui == UIBeingViewed.Rogue);
    }

    private void HandleBuffSelectedEvent(object sender, RogueGraphNode node)
    {
        selectedNodes.Add(node);
        selectedBuffText.text += "\n" + node.buff.name;
        for(int i = 0; i < buffContainer.transform.childCount; i++)
        {
            GameObject buffCard = buffContainer.transform.GetChild(i).gameObject;
            buffCard.GetComponent<BuffSelectionController>().OnBuffSelectedEvent -= HandleBuffSelectedEvent;
            Destroy(buffCard);
        }
        
    }

    private const string NAME_ROGUE_UI = "RogueUI";
    private const string NAME_LEVEL_UP_BUTTON = "LevelUpButton";
    private const string NAME_SELECTED_BUFF_TEXT = "SelectedBuffText";
    private const string NAME_BUFF_CONTAINER = "BuffContainer";
}
