using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusRepository : MonoBehaviour
{
    private bool isViewingUi = false;

    public void SetIsViewingUi(bool isViewingUi)
    {
        this.isViewingUi = isViewingUi;
    }

    public bool GetIsViewingUi()
    {
        return isViewingUi;
    }
}
