using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AxeController : Weapon
{

    protected bool isLeftSide = false;




    protected override void FollowPlayerWithFloat()
    {
    
        idleBobTime += Time.deltaTime * idleBobSpeed;
        float bobOffset = Mathf.Sin(idleBobTime) * idleBobAmount;

        Vector3 targetPosition = player.transform.position + idleOffset + new Vector3(0, bobOffset, 0);

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);

        transform.rotation = Quaternion.Euler(0, 0, isLeftSide ? -45 : 45);
        transform.localScale = new Vector3(isLeftSide ? -1 : 1, 1, 1);




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
        


        StartCoroutine(PerformAxeAttack(targetPosition));

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator PerformAxeAttack(Vector2 targetPosition)
    {
        _audioEmitter.PlayClipFromCategory("WeaponAttack");

        Vector2 playerCenter = player.transform.position;
        isLeftSide = targetPosition.x < playerCenter.x; 

        float startAngle = isLeftSide ? 90f : 90f; 
        float endAngle = isLeftSide ? 225f : -45f; 
        float attackDuration = 0.1f;
        float resetDuration = 0.3f;

        float axeStartZ = isLeftSide ? -45f : 45f;
        float axeEndZ = isLeftSide ? 90f : -90f;

        // 攻击阶段
        float elapsedTime = 0f;
        while (elapsedTime < attackDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / attackDuration;

            // 角度插值
            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);

            // 计算斧头的旋转角度
            float currentAxeZ = Mathf.Lerp(axeStartZ, axeEndZ, t);

            // 根据角度计算斧头的位置
            Vector2 offset = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad)) * attackRange;
            transform.position = playerCenter + offset;

            // 设置斧头的旋转
            transform.localScale = new Vector3(isLeftSide ? -1 : 1, 1, 1); // 左侧时翻转 X 轴
            transform.rotation = Quaternion.Euler(0, 0, currentAxeZ);

            yield return null;
        }

        // 重置阶段
        elapsedTime = 0f;
        while (elapsedTime < resetDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / resetDuration;

            // 角度插值
            float currentAngle = Mathf.Lerp(endAngle, startAngle, t);

            // 计算斧头的旋转角度
            float currentAxeZ = Mathf.Lerp(axeEndZ, axeStartZ, t);

            // 根据角度计算斧头的位置
            Vector2 offset = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad)) * attackRange;
            transform.position = playerCenter + offset;

            transform.rotation = Quaternion.Euler(0, 0, currentAxeZ);


            yield return null;
        }

        isAttacking = false;
        targetEnemy = null;
    }
}
