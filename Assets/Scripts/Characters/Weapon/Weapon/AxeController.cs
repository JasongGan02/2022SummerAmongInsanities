using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeController : Weapon
{
   protected override void anim()
    {
        weaponAnmator = gameObject.AddComponent<Animator>();
        GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("PlayerAnimations/Weapon/Axe/Axe");
    }
}
