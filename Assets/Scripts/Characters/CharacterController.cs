using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Threading;
using UnityEditor.Build;
using System.Linq;

public abstract class CharacterController : MonoBehaviour, IEffectableObject
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
    protected float AtkRange;
    protected float JumpForce;
    protected CharacterController[] Hatred;

    protected List<EffectObject> effects;

    public List<EffectObject> Effects
    {
        get { return effects; }
        set { effects = value; }
    }

    public CharacterObject CharacterStats
    {
        get { return characterStats; }
        set { characterStats = value; }
    }

    protected virtual void Awake()
    {
        effects = new List<EffectObject>();
    }

    protected virtual void Update()
    {
        ExecuteEffects();
    }

    public void ExecuteEffects()
    {
        foreach (EffectObject effect in Effects)
        {
            effect.ExecuteEffect(this);
        }
        Effects.Clear();
    }


    public virtual void Initialize(CharacterObject characterObject)
    {
        this.characterStats = characterObject;
        Type controllerType = GetType();
        Type objectType = characterObject.GetType();

        // Get all the fields of the controller type
        FieldInfo[] controllerFields = controllerType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        // Iterate over the fields and set their values from the object
        foreach (FieldInfo controllerField in controllerFields)
        {
            // Try to find a matching field in the object
            FieldInfo objectField = objectType.GetField(controllerField.Name);
            // If there's a matching field, set the value
            if (objectField != null && objectField.FieldType == controllerField.FieldType)
            {
                controllerField.SetValue(this, objectField.GetValue(characterObject));
            }
        }

        // Get all the properties of the controller type
        PropertyInfo[] controllerProperties = controllerType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);

        // Iterate over the properties and set their values from the object
        foreach (PropertyInfo controllerProperty in controllerProperties)
        {
            // Try to find a matching property in the object
            PropertyInfo objectProperty = objectType.GetProperty(controllerProperty.Name);

            // If there's a matching property, set the value
            if (objectProperty != null && objectProperty.PropertyType == controllerProperty.PropertyType && objectProperty.CanRead)
            {
                controllerProperty.SetValue(this, objectProperty.GetValue(characterObject));
            }
        }
    }

    public virtual void takenDamage(float dmg)
    {
        HP -= dmg;
        if (HP <= 0)
        {
            death();
        }
        if (HP > characterStats.HP)
        {
            HP = characterStats.HP;
        }
        if(dmg > 0)
            StartCoroutine(FlashRed());
    }

    private System.Collections.IEnumerator FlashRed()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        for (int i = 0; i < 5; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(2 / (5 * 2));
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(2 / (5* 2));
        }
    }

    public void ChangeCurStats(float dHP, float dAtkDamage, float dAtkInterval, float dMovingSpeed, float dAtkRange, float dJumpForce)
    {
        this.takenDamage(-dHP);
        this.AtkDamage += dAtkDamage;
        this.AtkInterval += dAtkInterval;
        this.MovingSpeed += dMovingSpeed;
        this.AtkRange += dAtkRange;
        this.JumpForce += dJumpForce;
    }

    public void ChangeCharStats(float dHP, float dAtkDamage, float dAtkInterval, float dMovingSpeed, float dAtkRange, float dJumpForce)
    {
        ChangeCurStats(dHP, dAtkDamage, dAtkInterval, dMovingSpeed, dAtkRange, dJumpForce);
        characterStats.HP += dHP;
        characterStats.AtkDamage += dAtkDamage;
        characterStats.AtkInterval += dAtkInterval;
        characterStats.MovingSpeed += dMovingSpeed;
        characterStats.AtkRange += dAtkRange;
        characterStats.JumpForce += dJumpForce;
    }


    public abstract void death();



    public CharacterObject GetCharacterObject(){
        return characterStats;
    }

    protected void OnObjectDestroyed(bool isPlacedByPlayer)
    {
        var drops = characterStats.GetDroppedGameObjects(isPlacedByPlayer);
        foreach (GameObject droppedItem in drops)
        {
            //droppedItem.transform.parent = gameObject.transform.parent;
            droppedItem.transform.position = gameObject.transform.position;
            droppedItem.GetComponent<Rigidbody2D>().AddTorque(10f);
        }
    }

}

