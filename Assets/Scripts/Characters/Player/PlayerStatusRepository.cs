using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerStatusRepository
{
    private static bool isViewingUi = false;

    public static void SetIsViewingUi(bool isViewingUi)
    {
        PlayerStatusRepository.isViewingUi = isViewingUi;
    }

    public static bool GetIsViewingUi()
    {
        return isViewingUi;
    }
}
