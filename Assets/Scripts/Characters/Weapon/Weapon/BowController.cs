using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BowController : RangedWeaponController
{
    private Camera mainCamera;
    private float attackInterval = 0.5f;
    private float timeSinceLastAttack = 0f;
    private float chargeTime = 0f; // Time for which the button has been held
    private const float maxChargeTime = 2.0f; // Max time for full charge
    


    public override void Start()
    {
        base.Start();
        startPosition = transform;
        mainCamera = Camera.main;
       
    }

    public override void Update()
    {
     
        
        if (Input.GetMouseButtonDown(0))
        {
            am.playWeaponAudio(am.bow);

            chargeTime = 0f; // Start charging when the button is pressed
        }

        if (Input.GetMouseButton(0))
        {
            
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Min(chargeTime, maxChargeTime); // Cap chargeTime at maxChargeTime
        }

        if (Input.GetMouseButtonUp(0) && timeSinceLastAttack >= attackInterval)
        {
            FireProjectile(null);
            timeSinceLastAttack = 0f;
        }

        timeSinceLastAttack += Time.deltaTime;
    }

    public override void FireProjectile(GameObject target)
    {
        if (!inventory.ConsumeItem(projectileObject, 1))
            return;
        // Calculate the force and damage based on charge time
        float chargePercent = chargeTime / maxChargeTime;
        float force = chargePercent * AttackRange * 5;
        float damage = chargePercent * characterController.AtkDamage;
        //GameObject arrow = ProjectilePoolManager.Instance.GetProjectile(projectileObject.getPrefab());
        GameObject arrow = PoolManager.Instance.Get(projectileObject);
        var playerBowArrow = arrow.GetComponent<PlayerBowProjectile>();
        if (playerBowArrow != null)
        {
            arrow.transform.position = startPosition.transform.position;
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            playerBowArrow.Initialize(characterController, projectileObject, force, damage);
            playerBowArrow.Launch(mousePosition); // Launch without a specific target
            am.playWeaponAudio(am.shoot);
        }
    }



    public override void OnCollisionEnter2D(Collision2D collision)
    {


    }

 
}