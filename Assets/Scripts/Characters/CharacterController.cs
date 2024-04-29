using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Threading;
using UnityEditor.Build;
using System.Linq;

[RequireComponent(typeof(AudioEmitter))]
public abstract class CharacterController : MonoBehaviour, IEffectableObject, IPoolableObjectController, IDamageable, IDamageSource, IAudioable
{
     /*any kind of "permanent" variable will go to this characterObject. e.g. And to refer the MaxHP, call characterStats.HP. Do not change any value 
    in the scriptable objects by directly calling them. e.g. characterStats.HP +=1; this is not allowed as this HP is the MaxHP of a character. They should only be modified through other 
    scripts like roguelike system 
    Additionally, be aware of type casting because this stats is always pointing another type actually like VillagerObject in actual cases. Thus, when you want to access a specific value here 
    that only belongs to VillagerObject, type casting it first before you access to the variable: ((VillagerObject) characterStats).villagerSpecificVariable
    */
    protected CharacterObject characterStats; 


    //You will obtain and maintain the run-tim variables here by calling HP straightly for example. HP -= dmg shown below. 
    protected float _HP;
    protected float _armor;
    protected float _atkDamage;
    protected float _atkSpeed;
    protected float _movingSpeed;
    protected float _atkRange;
    protected float _criticalMultiplier;
    protected float _criticalChance;
    protected float _jumpForce;
    protected int _totalJumps;
    protected Drop[] drops;
    protected List<TextAsset> Hatred;
    protected DamageDisplay damageDisplay;

    protected AudioEmitter _audioEmitter;

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

    public BaseObject PoolableObject => characterStats;

    protected virtual void Awake()
    {
        effects = new List<EffectObject>();
        _audioEmitter = GetComponent<AudioEmitter>();
        
        damageDisplay = gameObject.AddComponent<DamageDisplay>();
       
    }

    protected virtual void OnEnabled()
    {

    }
    protected virtual void Update()
    {
        ExecuteEffects();
    }

    public void ExecuteEffects()
    {
        if (Effects != null)
        {
            foreach (EffectObject effect in Effects)
            {
                if (effect != null)
                {
                    effect.ExecuteEffect(this);
                }
            }
            Effects.Clear();
        }
    }



    public virtual void Initialize(CharacterObject characterObject)
    {
        characterStats = characterObject;
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

    public virtual void ApplyHPChange(float dmg)
    {
        _HP -= dmg;

        if (_HP <= 0)
        {
            death();
        }
        if (_HP > characterStats._HP) //hp cap
        {
            _HP = characterStats._HP;
        }
        if (dmg > 0)
        {
            if (gameObject.activeSelf)
                StartCoroutine(FlashRed());
        }
        
    }

    //Implementation of IDamageSource
    #region
    public float DamageAmount => _atkDamage;
    public float CriticalChance => _criticalChance;
    public float CriticalMultiplier => _criticalMultiplier;
    public virtual void ApplyDamage(IDamageable target)
    {
        float damageDealt = target.CalculateDamage(DamageAmount, CriticalChance, CriticalMultiplier);
        target.TakeDamage(damageDealt, this);
    }
    #endregion

    //----------------------------------------------------------------------------------------------------------

    // Implementation of IDamageable
    #region
    public virtual void TakeDamage(float amount, IDamageSource damageSource)
    {
        Transform damagedTransfrom = this.transform;
        if (damageDisplay != null)
            damageDisplay.ShowDamage(amount, damagedTransfrom);
        else
            Debug.Log("displayNullDamage");

        ApplyHPChange(amount);
    }

    public virtual float CalculateDamage(float incomingAtkDamage, float attackerCritChance, float attackerCritDmgCoef)
    {
        // Determine if this is a critical hit
        bool isCritical = UnityEngine.Random.value < attackerCritChance;

        // Calculate base damage with critical hit consideration
        float baseDamage = isCritical ? incomingAtkDamage * attackerCritDmgCoef : incomingAtkDamage;

        // Calculate damage reduction from armor
        float damageReduction = _armor / (_armor + 100);
        float finalDamage = baseDamage * (1 - damageReduction);

        return finalDamage;
    }
    #endregion

    public IEnumerator FlashRed()
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

    public void ChangeCurStats(StatModifications mods)
    {
        ApplyHPChange(-mods.HP);
        _atkDamage += mods.AtkDamage;
        _atkSpeed += mods.AtkInterval;
        _movingSpeed += mods.MovingSpeed;
        _atkRange += mods.AtkRange;
        _jumpForce += mods.JumpForce;
        _totalJumps += mods.TotalJumps;
        _armor += mods.Armor;
        _criticalMultiplier += mods.CriticalMultiplier;
        _criticalChance += mods.CriticalChance;
        EvokeStatsChange();
    }

    public void ChangeCharStats(StatModifications mods)
    {
        characterStats._HP += mods.HP;
        characterStats._atkDamage += mods.AtkDamage;
        characterStats._atkSpeed += mods.AtkInterval;
        characterStats._movingSpeed += mods.MovingSpeed;
        characterStats._atkRange += mods.AtkRange;
        characterStats._jumpForce += mods.JumpForce;
        characterStats._totalJumps += mods.TotalJumps;
        characterStats._armor += mods.Armor;
        characterStats._criticalMultiplier += mods.CriticalMultiplier;
        characterStats._criticalChance += mods.CriticalChance;
        //ChangeCurStats(mods);
    }

    protected virtual void EvokeStatsChange()
    {

    }

    protected virtual void death()
    {
        PoolManager.Instance.Return(this.gameObject, characterStats);
        OnObjectReturned(false);
    }



    public CharacterObject GetCharacterObject(){
        return characterStats;
    }

    protected virtual void OnObjectReturned(bool isDestroyedByPlayer)
    {
        var drops = characterStats.GetDroppedGameObjects(isDestroyedByPlayer, gameObject.transform.position);
    }

    public virtual void Reinitialize()
    {
        Initialize(characterStats);
    }

    public float AtkDamage
    {
        get { return _atkDamage; }
        set { _atkDamage = value; }
    }

    public float AtkRange
    {
        get { return _atkRange; }
        set { _atkRange = value; }
    }

    public AudioEmitter GetAudioEmitter()
    {
        return _audioEmitter;
    }
}

