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

        Vector2 targetDirection = (targetPosition - (Vector2)player.transform.position).normalized;
        bool shouldFlip = targetPosition.x < player.transform.position.x;
        if (shouldFlip)
        {
            transform.localScale = new Vector3(-2, 2, 2); 
        }
        else
        {
            transform.localScale = new Vector3(2, 2, 2); 
        }

        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg - 45;
        if (shouldFlip)
        {
            targetAngle -= 90;
        }

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 0, targetAngle); 

        float rotateTime = 0.0f;
        float rotationDuration = 0.2f; 
        while (rotateTime < 1.0f)
        {
            rotateTime += Time.deltaTime / rotationDuration;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, rotateTime);
            yield return null; 
        }

        yield return new WaitForSeconds(0.5f);

        float initialAngle = transform.rotation.eulerAngles.z;
        float raisedAngleAdjustment = shouldFlip ? -150f : 150f; 
        float raisedAngle = initialAngle + raisedAngleAdjustment;

        Vector2 startPosition = transform.position;
        Vector2 upwardMovement = new Vector2(shouldFlip ? -0.5f : 0.5f, 0.5f);

        float aimDuration = 0.5f; 
        float startTime = Time.time;

        while (Time.time - startTime < aimDuration)
        {
            float fracJourney = (Time.time - startTime) / aimDuration;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(initialAngle, raisedAngle, fracJourney));
            transform.position = Vector2.Lerp(startPosition, startPosition + upwardMovement, rotateTime);
            yield return null;
        }

        StartCoroutine(PerformAxeAttack(targetPosition));

        yield return new WaitForSeconds(0.2f);
    }


    IEnumerator PerformAxeAttack(Vector2 targetPosition)
    {
        Vector2 startPosition = transform.position;
        bool shouldFlip = transform.localScale.x < 0; 

        // 计算旋转开始和结束角度
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
        float totalDuration = 0.1f; // 攻击动作总时长
        Vector2 endPosition = targetPosition; // 直接使用怪物位置作为结束位置

        while (journeyTime < 1.0f)
        {
            journeyTime += Time.deltaTime / totalDuration;
            float heightFactor = Mathf.Sin(journeyTime * Mathf.PI); // 正弦波计算高度，生成弧线运动

            // 插值计算当前位置和旋转
            transform.position = Vector2.Lerp(startPosition, endPosition, journeyTime) + Vector2.up * heightFactor * 0.5f;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(startAngle, endAngle, journeyTime));

            yield return null;
        }

        // 攻击完成后的处理
        isAttacking = false;
        targetEnemy = null;
    }





}
