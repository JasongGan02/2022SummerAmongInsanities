using System.Collections;
using System.Collections.Generic;
public struct StatModifications
{
    public float HP;
    public float AtkDamage;
    public float AtkInterval;
    public float MovingSpeed;
    public float AtkRange;
    public float JumpForce;
    public int TotalJumps;
    public float Armor;
    public float CriticalMultiplier;
    public float CriticalChance;

    // Constructor for easy instantiation
    public StatModifications(float hp = 0, float atkDamage = 0, float atkInterval = 0, float movingSpeed = 0, float atkRange = 0, float jumpForce = 0, int totalJumps = 0, float armor = 0, float criticalMultiplier = 0, float criticalChance = 0)
    {
        HP = hp;
        AtkDamage = atkDamage;
        AtkInterval = atkInterval;
        MovingSpeed = movingSpeed;
        AtkRange = atkRange;
        JumpForce = jumpForce;
        TotalJumps = totalJumps;
        Armor = armor;
        CriticalMultiplier = criticalMultiplier;
        CriticalChance = criticalChance;
    }
}

