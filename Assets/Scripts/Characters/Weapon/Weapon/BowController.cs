using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class BowController : RangedWeaponController
{




    protected override void DetectAndAttackEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.transform.position, attackRange * 1.2f);
        float minDistance = float.MaxValue;
        Transform closestTarget = null;

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<EnemyController>(out EnemyController enemy))
            {
                float distance = Vector2.Distance(player.transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTarget = enemy.transform;

                }
            }
        }
        targetEnemy = closestTarget;
        if (targetEnemy != null && !isAttacking)
        {
            StartCoroutine(Attack(targetEnemy.gameObject));
        }
    }

    IEnumerator Attack(GameObject target)
    {
        if (isAttacking)
            yield break;

        isAttacking = true;

 
        float adjustedWindupTime = Mathf.Clamp(attackCycleTime * windupRatio, minWindupTime, maxWindupTime);
        float adjustedFollowThroughTime = Mathf.Clamp(attackCycleTime * followThroughRatio, minFollowThroughTime, maxFollowThroughTime);
        float adjustedIntervalTime = attackCycleTime - (adjustedWindupTime + adjustedFollowThroughTime);


        _audioEmitter.PlayClipFromCategory("BowCharge");
        yield return new WaitForSeconds(adjustedWindupTime);


        FireProjectiles(target);

   
        yield return new WaitForSeconds(adjustedFollowThroughTime);


        float intervalStartTime = Time.time;
        while (Time.time - intervalStartTime < adjustedIntervalTime)
        {
            Flip();
            yield return null;
        }

        isAttacking = false;
        targetEnemy = null;
    }

    public override void FireProjectiles(GameObject target)
    {
        if (!inventory.ConsumeItem(projectileObject, 1))
            return;

        float force = attackRange*2;
        float damage = characterController.CurrentStats.attackDamage;

        GameObject arrow = PoolManager.Instance.Get(projectileObject);
        arrow.transform.position = startPosition.transform.position;
        var playerBowArrow = arrow.GetComponent<PlayerBowProjectileController>();

        if (playerBowArrow != null)
        {
            playerBowArrow.Initialize(characterController, projectileObject, force, damage, knockbackForce);
            _audioEmitter.PlayClipFromCategory("ShootArrow");
            playerBowArrow.Launch(target, startPosition);
        }
    }


    protected override void KnockbackEnemy(Collider2D enemy)
    {
        
    }



}