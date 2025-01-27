using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AxeController : Weapon
{

    protected bool isLeftSide = false;


    protected float windupTime => Mathf.Clamp(attackCycleTime * windupRatio, minWindupTime, maxWindupTime);
    protected float followThroughTime => Mathf.Clamp(attackCycleTime * followThroughRatio, minFollowThroughTime, maxFollowThroughTime);
    protected float intervalTime => attackCycleTime - (windupTime + followThroughTime);

    protected override void FollowPlayerWithFloat()
    {
        idleBobTime += Time.deltaTime * idleBobSpeed;
        float bobOffset = Mathf.Sin(idleBobTime) * idleBobAmount;

        Vector3 targetPosition = player.transform.position + idleOffset + new Vector3(0, bobOffset, 0);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);

    
        if (targetEnemy != null)
        {
            Vector2 directionToEnemy = (targetEnemy.position - player.transform.position).normalized;
            transform.rotation = Quaternion.Euler(0, 0, directionToEnemy.x < 0 ? -45 : 45);
            transform.localScale = new Vector3(directionToEnemy.x < 0 ? -1 : 1, 1, 1);
        }
        else
        {
            if (playerMovement != null)
            {
                transform.rotation = Quaternion.Euler(0, 0, !playerMovement.facingRight ? -45 : 45); 
                transform.localScale = new Vector3(!playerMovement.facingRight ? -1 : 1, 1, 1); 
            }
        }
    }

    protected override void DetectAndAttackEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.transform.position, attackRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<EnemyController>(out EnemyController enemy))
            {
                targetEnemy = hit.transform;

                StartCoroutine(PrepareAttack(hit.transform.position));
                break;
            }
        }
    }

    IEnumerator PrepareAttack(Vector2 targetPosition)
    {
        if (isAttacking)
            yield break;

        isAttacking = true;
        Vector2 playerCenter = player.transform.position;
        isLeftSide = targetPosition.x < playerCenter.x;


        yield return StartCoroutine(PerformWindup(targetPosition));


        yield return StartCoroutine(ReturnToPlayer());


        float intervalStartTime = Time.time;
        while (Time.time - intervalStartTime < intervalTime)
        {
            FollowPlayerWithFloat();
            yield return null;
        }

        isAttacking = false;
        targetEnemy = null;
    }

    IEnumerator PerformWindup(Vector2 targetPosition)
    {
        hasDealtDamage = false;
        _audioEmitter.PlayClipFromCategory("WeaponAttack");

        Vector2 playerCenter = player.transform.position;
        float startAngle = isLeftSide ? 90f : 90f;
        float endAngle = isLeftSide ? 225f : -45f;

        float axeStartZ = isLeftSide ? -45f : 45f;
        float axeEndZ = isLeftSide ? 90f : -90f;

        float elapsedTime = 0f;
        while (elapsedTime < windupTime) 
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / windupTime;

            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
            float currentAxeZ = Mathf.Lerp(axeStartZ, axeEndZ, t);

            Vector2 offset = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad)) * attackRange;
            transform.position = playerCenter + offset;

            transform.rotation = Quaternion.Euler(0, 0, currentAxeZ);

            yield return null;
        }
    }


    IEnumerator ReturnToPlayer()
    {
        Vector2 playerCenter = player.transform.position;
        float startAngle = isLeftSide ? 225f : -45f;
        float endAngle = isLeftSide ? 90f : 90f;

        float axeStartZ = isLeftSide ? 90f : -90f;
        float axeEndZ = isLeftSide ? -45f : 45f;

        float elapsedTime = 0f;
        while (elapsedTime < followThroughTime) 
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / followThroughTime;

            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
            float currentAxeZ = Mathf.Lerp(axeStartZ, axeEndZ, t);

            Vector2 offset = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad)) * attackRange;
            transform.position = playerCenter + offset;

            transform.rotation = Quaternion.Euler(0, 0, currentAxeZ);

            yield return null;
        }
    }
}
