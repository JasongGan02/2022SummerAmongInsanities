using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SaveSlot : MonoBehaviour
{
   [Header("Profile")]
   [SerializeField] private string profileId = "";
   [Header("Content")]
   [SerializeField] private GameObject noDataContent;
   [SerializeField] private GameObject hasDataContent;
   [SerializeField] private TextMeshProUGUI percentageCompleteText;
   [SerializeField] private TextMeshProUGUI deathCountText;

   public void SetData(GameData data)
   {
        if (data == null)
        {
            noDataContent.SetActive(true);
            hasDataContent.SetActive(false);
        }
        else
        {
            noDataContent.SetActive(false);
            hasDataContent.SetActive(true);
            //percentageCompleteText.text = data.percentageComplete.ToString() + "%";
            deathCountText.text = "DEATH COUNT: " + data.deathCount;
        }
   }
   
   public string GetProfileId()
   {
       return this.profileId;
   }
}
