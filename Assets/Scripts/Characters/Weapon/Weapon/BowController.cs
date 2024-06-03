using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class BowController : RangedWeaponController
{
    Vector3 directionToEnemy;
    
    private void Flip()
    {

        Vector3 theScale = transform.localScale;
        if (directionToEnemy.x > 0)
        {

            theScale.y = 1.5f;
            theScale.x = 1.5f;
            transform.localScale = theScale;
            transform.position = player.transform.position + new Vector3(1f, 0, 0);

        }
        else
        {
            theScale.y = -1.5f;
            theScale.x = -1.5f;
            transform.localScale = theScale;
            transform.position = player.transform.position + new Vector3(-1f, 0, 0);
        }
    }
    public override void Update()
    {
        Flip();
        startPosition = transform;

        if (!isAttacking && inventory.FindItemCount(ProjectileObject)>=1)
        {
            DetectAndAttackEnemy();
        }

    }


    protected override void DetectAndAttackEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.transform.position, attackRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<EnemyController>(out EnemyController enemy))
            {

                directionToEnemy = (enemy.transform.position - player.transform.position).normalized;
                StartCoroutine(PrepareAttack(hit.gameObject));
                break;

            }
        }

    }

    IEnumerator PrepareAttack(GameObject target)
    {
        yield return new WaitForSeconds(1f);
        if (isAttacking)
            yield break;

        isAttacking = true;

        _audioEmitter.PlayClipFromCategory("BowCharge");
        yield return new WaitForSeconds(1f);
        FireProjectile(target);


    }
    


    public override void FireProjectile(GameObject target)
    {

        if (!inventory.ConsumeItem(projectileObject, 1))
            return;
        // Calculate the force and damage based on charge time

        float force =  AttackRange/8;
        float damage =  characterController.AtkDamage;
        //GameObject arrow = ProjectilePoolManager.Instance.GetProjectile(projectileObject.getPrefab());
        
        GameObject arrow = PoolManager.Instance.Get(projectileObject);
        arrow.transform.SetParent(transform, true);
        arrow.transform.position = startPosition.transform.position;
        var playerBowArrow = arrow.GetComponent<PlayerBowProjectile>();
      
        if (playerBowArrow != null)
        {
            playerBowArrow.Initialize(characterController, projectileObject, force, damage,knockbackForce);
            _audioEmitter.PlayClipFromCategory("ShootArrow");
            playerBowArrow.Launch(target,startPosition); 
            isAttacking = false;

        }
    }


    protected override void KnockbackEnemy(Collider2D enemy)
    {
        
    }



}