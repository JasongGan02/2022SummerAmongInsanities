using System;
using System.Collections;
using UnityEngine;

public class DetonateChargeEffectController : EffectController
{
    private DetonateChargeEffectObject chargeEffectObject;
    private Weapon weapon;
    private bool isCharged = false;

    private GameObject chargeUpVFX; // VFX for ChargeUp
    private GameObject chargeReadyVFX; // VFX for ChargeReady

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
        // Start the ChargeUp VFX
        StartChargeUpVFX();

        // Start the charge-up process
        StartCoroutine(ChargeCountdownCoroutine());
    }

    private IEnumerator ChargeCountdownCoroutine()
    {
        yield return new WaitForSeconds(chargeEffectObject.chargeDuration);

        // Transition to ChargeReady state
        isCharged = true;

        // Stop ChargeUp VFX and start ChargeReady VFX
        StopChargeUpVFX();
        StartChargeReadyVFX();

        Debug.Log($"Weapon {weapon.name} is ready for detonate effect.");
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

        // Consume the charge
        isCharged = false;

        // Stop ChargeReady VFX
        StopChargeReadyVFX();

        Debug.Log($"Weapon applied detonate hit to {target}.");

        // Restart the charge countdown
        StartChargeCountdown();
    }

    private void StartChargeUpVFX()
    {
        var vfxData = effectObject.vfxList.Find(vfx => vfx.name == "ChargeUp");
        if (vfxData != null && vfxData.vfxPrefab != null)
        {
            chargeUpVFX = Instantiate(vfxData.vfxPrefab, transform.position, Quaternion.identity, vfxData.attachToTarget ? transform : null);
            Debug.Log("Started ChargeUp VFX.");
        }
    }

    private void StopChargeUpVFX()
    {
        if (chargeUpVFX != null)
        {
            Destroy(chargeUpVFX);
            Debug.Log("Stopped ChargeUp VFX.");
        }
    }

    private void StartChargeReadyVFX()
    {
        var vfxData = effectObject.vfxList.Find(vfx => vfx.name == "ChargeReady");
        if (vfxData != null && vfxData.vfxPrefab != null)
        {
            chargeReadyVFX = Instantiate(vfxData.vfxPrefab, transform.position, Quaternion.identity, vfxData.attachToTarget ? transform : null);
            Debug.Log("Started ChargeReady VFX.");
        }
    }

    private void StopChargeReadyVFX()
    {
        if (chargeReadyVFX != null)
        {
            Destroy(chargeReadyVFX);
            Debug.Log("Stopped ChargeReady VFX.");
        }
    }
}
