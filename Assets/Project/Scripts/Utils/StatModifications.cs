using System;

[Serializable]
public struct StatModifications
{
    public float hp;
    public float attackDamage;
    public float attackInterval;
    public float movingSpeed;
    public float attackRange;
    public float jumpForce;
    public int totalJumps;
    public float armor;
    public float criticalMultiplier;
    public float criticalChance;

    // Constructor for easy instantiation
    public StatModifications(
        float hp = 0, 
        float attackDamage = 0, 
        float attackInterval = 0, 
        float movingSpeed = 0, 
        float attackRange = 0, 
        float jumpForce = 0, 
        int totalJumps = 0, 
        float armor = 0, 
        float criticalMultiplier = 0, 
        float criticalChance = 0)
    {
        this.hp = hp;
        this.attackDamage = attackDamage;
        this.attackInterval = attackInterval;
        this.movingSpeed = movingSpeed;
        this.attackRange = attackRange;
        this.jumpForce = jumpForce;
        this.totalJumps = totalJumps;
        this.armor = armor;
        this.criticalMultiplier = criticalMultiplier;
        this.criticalChance = criticalChance;
    }
}