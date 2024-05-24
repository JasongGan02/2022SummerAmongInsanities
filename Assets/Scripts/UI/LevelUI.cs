using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    GameObject player;
    public TextMeshProUGUI leveltext;
    public TextMeshProUGUI EXPtext;

    int level;
    float EXP;

    // Start is called before the first frame update
    void Start()
    {
        leveltext.text = "";
        EXPtext.text = "EXP: ";
    }

    // Update is called once per frame
    void Update()
    {
        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            //Debug.Log("Player found!");
            level = player.GetComponent<PlayerController>().GetLevel();
            EXP = player.GetComponent<PlayerController>().GetEXP();

            
            EXPtext.text = "EXP: " + EXP.ToString();
        }
        else
        {
            //Debug.Log("Player not found!");
        }
    }
}
