using UnityEngine;

public class CharUpgradeEffectController : EffectController
{
    private CharUpgradeEffectObject upgradeEffect;

    protected override void StartEffect()
    {
        upgradeEffect = effectObject as CharUpgradeEffectObject;
        if (upgradeEffect != null)
        {
            ApplyUpgrades();
        }
    }

    private void ApplyUpgrades()
    {
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.ChangeMaxStats(upgradeEffect.statModifications);
        }
    }
}