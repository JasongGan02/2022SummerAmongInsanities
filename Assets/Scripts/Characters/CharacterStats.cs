using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public  class CharacterStats
{
    public float hp;
    public float attackDamage;
    public float attackInterval;
    public float attackRange;
    public float armor;
    public float criticalChance;
    public float criticalMultiplier;
    public float movingSpeed;
    public float jumpForce;
    public int totalJumps;

    // Copy the values from another CharacterStats instance
    public virtual void CopyFrom(CharacterStats source)
    {
        hp = source.hp;
        attackDamage = source.attackDamage;
        attackInterval = source.attackInterval;
        movingSpeed = source.movingSpeed;
        attackRange = source.attackRange;
        jumpForce = source.jumpForce;
        totalJumps = source.totalJumps;
        armor = source.armor;
        criticalMultiplier = source.criticalMultiplier;
        criticalChance = source.criticalChance;
    }

    // Reset to the values of another CharacterStats instance
    public void ResetTo(CharacterStats source)
    {
        CopyFrom(source);
    }

    // Add stats from another CharacterStats instance
    public virtual void AddStats(CharacterStats mods)
    {
        hp += mods.hp;
        attackDamage += mods.attackDamage;
        attackInterval += mods.attackInterval;
        movingSpeed += mods.movingSpeed;
        attackRange += mods.attackRange;
        jumpForce += mods.jumpForce;
        totalJumps += mods.totalJumps;
        armor += mods.armor;
        criticalMultiplier += mods.criticalMultiplier;
        criticalChance += mods.criticalChance;
    }
}

[Serializable]
public class PlayerStats : CharacterStats
{
    public float respawnTimeInterval;

    // Copy the values from another VillagerStats instance
    public override void CopyFrom(CharacterStats source)
    {
        base.CopyFrom(source);
        if (source is PlayerStats playerSource)
        {
            respawnTimeInterval = playerSource.respawnTimeInterval;
        }
    }
    
    // Add stats from another VillagerStats instance
    public override void AddStats(CharacterStats mods)
    {
        base.AddStats(mods);
        if (mods is PlayerStats playerSource)
        {
            respawnTimeInterval += playerSource.respawnTimeInterval;
        }
    }
}

[Serializable]
public class EnemyStats : CharacterStats
{
    public float sensingRange;

    // Copy the values from another VillagerStats instance
    public override void CopyFrom(CharacterStats source)
    {
        base.CopyFrom(source);
        if (source is EnemyStats enemySource)
        {
            sensingRange = enemySource.sensingRange;
        }
    }
    
    // Add stats from another VillagerStats instance
    public override void AddStats(CharacterStats mods)
    {
        base.AddStats(mods);
        if (mods is EnemyStats enemySource)
        {
            sensingRange += enemySource.sensingRange;
        }
    }
}

[Serializable]
public class TowerStats : CharacterStats
{
    [Header("Construction Parameter")]
    public int energyCost;
    public Quaternion rotateAngle;//a fixed amount that determines the rotation type of a tower
    [HideInInspector] public Quaternion curAngle =  Quaternion.Euler(0, 0, 0);

    // Copy the values from another VillagerStats instance
    public override void CopyFrom(CharacterStats source)
    {
        base.CopyFrom(source);
        if (source is TowerStats towerSource)
        {
            energyCost = towerSource.energyCost;
            rotateAngle = towerSource.rotateAngle;
        }
    }
    
    // Add stats from another VillagerStats instance
    public override void AddStats(CharacterStats mods)
    {
        base.AddStats(mods);
        if (mods is TowerStats towerSource)
        {
            energyCost += towerSource.energyCost;
        }
    }
}