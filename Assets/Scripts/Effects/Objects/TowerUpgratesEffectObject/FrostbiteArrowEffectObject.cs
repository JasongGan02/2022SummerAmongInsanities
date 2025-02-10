using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Tower Upgrades/FrostbiteArrowEffectObject")]
public class FrostbiteArrowEffectObject : EffectObject, IDamageSource
{
    [Header("FrostbiteArrowEffectObject Fields")]
    public float damagePerStacks;


    public GameObject SourceGameObject => null;

    public void ApplyDamage(IDamageable target)
    {
        var onChilledEffect = (target as MonoBehaviour).GetComponent<OnChilledEffectController>();
        if (onChilledEffect == null)
        {
            return;
        }

        float DamageAmount = damagePerStacks * onChilledEffect.stackCount;
        float damageDealt = target.CalculateDamage(DamageAmount, 0, 0);
        target.TakeDamage(damageDealt, this);
    }

    public override void ExecuteEffect(IEffectableController effectedGameController)
    {
        ApplyDamage(effectedGameController as IDamageable);
    }

    protected override async void OnInitializeEffect()
    {
        // Load the target tower asset via Addressables.
        RangedTowerObject archerTower = await AddressablesManager.Instance.LoadAssetAsync<RangedTowerObject>("Assets/ScriptableObjects/CharacterObjects/TowerObject/ArcherTower.asset");

        if (archerTower != null)
        {
            // Check for an existing effect of the same type on the tower's projectile.
            var existingEffect = archerTower.projectileObject.onHitEffects.Find(effect =>
                effect.GetType() == GetType());

            if (existingEffect == null)
            {
                // If no existing effect is found, add this effect.
                archerTower.projectileObject.onHitEffects.Add(this);
            }
        }
        else
        {
            Debug.LogWarning("Failed to load ArcherTower asset for FrostbiteArrowEffectObject initialization.");
        }
    }
}