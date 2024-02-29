using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class CharUpgradeEffectController : EffectController
{
    private CharUpgradeEffectObject upgradeEffect;

    protected override void StartEffect()
    {
        upgradeEffect = effectObject as CharUpgradeEffectObject;
        if (upgradeEffect == null) return;

        ApplyUpgrades();
    }

    private void ApplyUpgrades()
    {
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            StatModifications mods = new StatModifications(
                upgradeEffect.dHP,
                upgradeEffect.dAtkDamage,
                upgradeEffect.dAtkInterval,
                upgradeEffect.dMovingSpeed,
                upgradeEffect.dAtkRange,
                upgradeEffect.dJumpForce,
                upgradeEffect.dTotalJumps,
                upgradeEffect.dArmor,
                upgradeEffect.dCriticalMultiplier,
                upgradeEffect.dCriticalChance
            );

            characterController.ChangeCharStats(mods);
        }
    }
}

