using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearController : Weapon
{
    
    protected override void anim()
    {
        weaponAnmator = gameObject.AddComponent<Animator>();
        GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("PlayerAnimations/Weapon/Spear/Spear");
    }


    public override void Start()
    {
        base.Start();
        this.transform.parent = player.transform;
    }
}
