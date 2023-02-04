using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterController : MonoBehaviour
{
     /*any kind of "permanent" variable will go to this characterObject. e.g. And to refer the MaxHP, call characterStats.HP. Do not change any value 
    in the scriptable objects by directly calling them. e.g. characterStats.HP +=1; this is not allowed as this HP is the MaxHP of a character. They should only be modified through other 
    scripts like roguelike system 
    Additionally, be aware of type casting because this stats is always pointing another type actually like VillagerObject in actual cases. Thus, when you want to access a specific value here 
    that only belongs to VillagerObject, type casting it first before you access to the variable: ((VillagerObject) characterStats).villagerSpecificVariable
    */
    protected CharacterObject characterStats; 


    //You will obtain and maintain the run-tim variables here by calling HP straightly for example. HP -= dmg shown below. 
    protected float HP;
    protected float AtkDamage;
    protected float AtkInterval;
    protected float MovingSpeed;
    protected Drop[] drops;

    public virtual void Initialize(CharacterObject character, float HP, float AtkDamage, float AtkInterval, float MovingSpeed)
    {
        this.characterStats = character;
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
