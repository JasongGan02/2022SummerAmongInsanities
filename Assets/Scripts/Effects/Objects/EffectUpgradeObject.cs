using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Effect Upgrade")]
public class EffectUpgradeObject : EffectObject
{
    [Header("EffectUpgradeObject Fields")] 
    public EffectObject upgradeableEffectObject;
    
    public override void InitializeEffectObject()
    {
        if (upgradeableEffectObject is IUpgradeableEffectObject IUpgradeableEffectObject)
        {
            IUpgradeableEffectObject.UpgradeLevel();
        }
        else
        {
            Debug.LogError("Put a non-upgradeable effect object in a Upgrade Effect");
        }
    }

}
