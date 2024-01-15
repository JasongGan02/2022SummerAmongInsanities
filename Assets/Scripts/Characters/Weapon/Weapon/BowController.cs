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
        Flip();
        Patrol();
        am.loopoffAudio();
        if (Input.GetMouseButtonDown(0))
        {
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
        Debug.Log(projectileObject);
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
        }
    }

    public override void Patrol()
    {
        if (playermovement.facingRight)
        {
            transform.position = player.transform.position + new Vector3(0.8f, 0, 0);
        }
        else
        {
            transform.position = player.transform.position - new Vector3(0.8f, 0, 0);
        }
    }

    public override void Flip()
    {
        if (playermovement.facingRight)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 315));
        }
        else
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 135));
        }

    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {


    }

 
}