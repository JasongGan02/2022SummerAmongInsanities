using System;
using System.Collections;
using UnityEngine;

public class DetonateChargeEffectController : EffectController
{
    private DetonateChargeEffectObject chargeEffectObject;
    private Weapon weapon;
    private bool isCharged = false;

    public void OnEnable()
    {
        //StopAllCoroutines();
        //isCharged = false;
        //StartChargeCountdown();
    }

    public override void Initialize(EffectObject effectObject)
    {
        chargeEffectObject = effectObject as DetonateChargeEffectObject;
        if (chargeEffectObject == null)
        {
            Debug.LogError("EffectObject is not a DetonateChargeEffectObject.");
            return;
        }

        base.Initialize(effectObject);

        weapon = GetComponent<Weapon>();
        if (weapon == null)
        {
            Debug.LogError("DetonateChargeEffectController requires a Weapon component.");
            return;
        }

        StartChargeCountdown();
    }

    private void StartChargeCountdown()
    {
        //Debug.Log($"Starting charge countdown for weapon: {weapon.name}");
        StartCoroutine(ChargeCountdownCoroutine());
    }

    private IEnumerator ChargeCountdownCoroutine()
    {
        yield return new WaitForSeconds(chargeEffectObject.chargeDuration);
        isCharged = true;
        //Debug.Log($"Weapon {weapon.name} is charged for detonate effect.");
    }

    public void ApplyDetonateHit(IEffectableController iEffectableController)
    {
        if (!isCharged)
        {
            Debug.Log("Weapon is not charged yet.");
            return;
        }
        MonoBehaviour target = iEffectableController as MonoBehaviour;
        var onFireEffect = target.GetComponent<OnFireEffectController>();
        if (onFireEffect == null)
        {
            Debug.Log("Target does not have OnFireEffect. Saving charge.");
            return;
        }

        // Apply the detonate effect
        var detonateEffectController = target.gameObject.AddComponent<OnDetonateEffectController>();
        detonateEffectController.Initialize(chargeEffectObject.detonateEffect);

        // Consume the charge and restart countdown
        isCharged = false;
        Debug.Log($"Weapon applied detonate hit to {target}.");
        StartChargeCountdown();
    }
}
