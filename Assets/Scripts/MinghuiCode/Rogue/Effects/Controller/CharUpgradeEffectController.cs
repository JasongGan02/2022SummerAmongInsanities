using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharUpgradeEffectController : EffectController
{
    protected float dHP;
    protected float dAtkDamage;
    protected float dAtkInterval;
    protected float dAtkRange;
    protected float dMovingSpeed;
    protected float dJumpForce;
    
    protected override void StartEffect()
    {
        // Start the effect or perform any necessary setup
        CharacterController characterController = this.gameObject.GetComponent<CharacterController>();
        characterController.ChangeCharStats(dHP, dAtkDamage, dAtkInterval, dMovingSpeed, dAtkRange, dJumpForce);
    }

    
}
