using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterController : MonoBehaviour
{
    private CharacterObject character;
    protected float HP;
    protected float AtkDamage;
    protected float AtkInterval;
    protected float MovingSpeed;
    protected Drop[] drops;

    public virtual void Initialize(CharacterObject character, float HP, float AtkDamage, float AtkInterval, float MovingSpeed)
    {
        this.character = character;
        this.HP = HP;
        this.AtkDamage = AtkDamage;
        this.AtkInterval = AtkInterval;
        this.MovingSpeed = MovingSpeed;

    }

    public virtual void takenDamage(float dmg)
    {
        HP -= dmg;
        if (HP <= 0)
        {
            death();
        }
    }

    public abstract void death();
}
