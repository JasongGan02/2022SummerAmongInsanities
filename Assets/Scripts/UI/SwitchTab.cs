using UnityEngine;
using UnityEngine.UI;

public class SwitchTab : MonoBehaviour
{
    public GameObject tab1Prefab; // Reference to the first tab prefab
    public GameObject tab2Prefab; // Reference to the second tab prefab
    public Button tab1Button;     // Reference to the first tab button
    public Button tab2Button;     // Reference to the second tab button

    void Start()
    {
        // Set the initial tab to be active
        ShowTab1();

        // Add listeners to the buttons
        tab1Button.onClick.AddListener(ShowTab1);
        tab2Button.onClick.AddListener(ShowTab2);
    }

    // Show Tab 1 and hide Tab 2
    void ShowTab1()
    {
        tab1Prefab.SetActive(true);  // Show the first tab
        tab2Prefab.SetActive(false); // Hide the second tab
    }

    // Show Tab 2 and hide Tab 1
    void ShowTab2()
    {
        tab1Prefab.SetActive(false); // Hide the first tab
        tab2Prefab.SetActive(true);  // Show the second tab
        Debug.Log("111");
    }
}
