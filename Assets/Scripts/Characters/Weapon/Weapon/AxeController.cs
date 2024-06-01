using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeController : Weapon
{
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
        if (targetEnemy == null)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

    }



    IEnumerator PrepareAttack(Vector2 targetPosition)
    {
        if (isAttacking)
            yield break;

        isAttacking = true;

        
        float rotationDuration = 0.2f;
        float aimDuration = 0.3f;
        float startTime = Time.time;
        float startRotationTime = Time.time;
        float raisedAngleAdjustment = 150f;

        Vector2 currentTargetPosition = targetPosition; 
        Vector2 targetDirection = (currentTargetPosition - (Vector2)player.transform.position).normalized;
        bool shouldFlip = currentTargetPosition.x < player.transform.position.x;

        while (Time.time - startRotationTime < rotationDuration)
        {
         
            
            transform.localScale = shouldFlip ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);

            
            float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg - 45;
            targetAngle += shouldFlip ? -90 : 0; 
            Quaternion endRotation = Quaternion.Euler(0, 0, targetAngle);
            float rotateTime = (Time.time - startRotationTime) / rotationDuration;
            transform.rotation = Quaternion.Lerp(transform.rotation, endRotation, rotateTime);

            
            transform.position = player.transform.position;

            yield return null;
        }

        
        float initialAngle = transform.rotation.eulerAngles.z;
        float raisedAngle = initialAngle + (shouldFlip ? -raisedAngleAdjustment : raisedAngleAdjustment);
        Vector2 startPosition = transform.position;
        Vector2 upwardMovement = new Vector2(shouldFlip ? -0.5f : 0.5f, 0.5f);

        while (Time.time - startTime < aimDuration)
        {
            float fracJourney = (Time.time - startTime) / aimDuration;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(initialAngle, raisedAngle, fracJourney));
            transform.position = Vector2.Lerp(startPosition, startPosition + upwardMovement, fracJourney);
            yield return null;
        }

        StartCoroutine(PerformAxeAttack(targetPosition));  

        yield return new WaitForSeconds(0.2f); 
    }



    IEnumerator PerformAxeAttack(Vector2 targetPosition)
    {
        _audioEmitter.PlayClipFromCategory("WeaponAttack");

        Vector2 startPosition = transform.position;
        bool shouldFlip = transform.localScale.x < 0; 

        
        float startAngle = transform.rotation.eulerAngles.z;
        float endAngle;

        if (shouldFlip)
        {
            endAngle = startAngle + 180;
        }
        else
        {
            endAngle = startAngle - 180;
        }

        float journeyTime = 0.0f;
        float totalDuration = 0.2f; 
        Vector2 endPosition = targetPosition; 

        while (journeyTime < 1.0f)
        {
            journeyTime += Time.deltaTime / totalDuration;
            float heightFactor = Mathf.Sin(journeyTime * Mathf.PI); 


            transform.position = Vector2.Lerp(startPosition, endPosition, journeyTime) + Vector2.up * heightFactor * 0.5f;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(startAngle, endAngle, journeyTime));

            yield return null;
        }


        isAttacking = false;
        targetEnemy = null;
    }





}
