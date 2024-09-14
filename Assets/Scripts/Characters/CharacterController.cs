using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

[RequireComponent(typeof(AudioEmitter))]
public abstract class CharacterController : MonoBehaviour, IEffectableController, IPoolableObjectController, IDamageable, IDamageSource, IAudioable
{
    protected CharacterObject characterObject; //characterObject.runtimeTemplateStats is the template max stats
    protected CharacterStats currentStats; //current stats
    protected AudioEmitter audioEmitter;
    protected DamageDisplay damageDisplay;
    protected List<TextAsset> hatred;

    public BaseObject PoolableObject => characterObject;
    public CharacterStats CurrentStats => currentStats;
    public CharacterObject CharacterObject => characterObject;

    protected virtual void Update()
    {
        
    }

    protected virtual void Awake()
    {
        audioEmitter = GetComponent<AudioEmitter>();
        
        damageDisplay = gameObject.AddComponent<DamageDisplay>();
       
    }
    
    public virtual void Initialize(CharacterObject characterObject)
    {
        this.characterObject = characterObject;
        currentStats = (CharacterStats)Activator.CreateInstance(characterObject.baseStats.GetType());
        currentStats.CopyFrom(characterObject.maxStats);
        hatred = characterObject.hatred;
        //CopyFieldsFromCharacterObject(characterObject);
    }

    private void CopyFieldsFromCharacterObject(CharacterObject characterObject)
    {
        Type controllerType = GetType();
        Type objectType = characterObject.GetType();

        FieldInfo[] controllerFields = controllerType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo controllerField in controllerFields)
        {
            FieldInfo objectField = objectType.GetField(controllerField.Name);
            if (objectField != null && objectField.FieldType == controllerField.FieldType)
            {
                controllerField.SetValue(this, objectField.GetValue(characterObject));
            }
        }

        PropertyInfo[] controllerProperties =
            controllerType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (PropertyInfo controllerProperty in controllerProperties)
        {
            PropertyInfo objectProperty = objectType.GetProperty(controllerProperty.Name);
            if (objectProperty != null && objectProperty.PropertyType == controllerProperty.PropertyType &&
                objectProperty.CanRead)
            {
                controllerProperty.SetValue(this, objectProperty.GetValue(characterObject));
            }
        }
    }

    public virtual void ApplyHPChange(float dmg)
    {
        currentStats.hp -= dmg;

        if (currentStats.hp <= 0)
        {
            Die();
        }
        else if (currentStats.hp > characterObject.maxStats.hp)
        {
            currentStats.hp = characterObject.maxStats.hp;
        }

        if (dmg > 0 && gameObject.activeSelf)
        {
            StartCoroutine(FlashRed());
        }
    }
    
    protected virtual void Die()
    {
        PoolManager.Instance.Return(this.gameObject, characterObject);
        OnObjectReturned(false);
    }

    protected virtual void OnObjectReturned(bool isDestroyedByPlayer)
    {
        var drops = characterObject.GetDroppedGameObjects(isDestroyedByPlayer, transform.position);
    }

    public virtual void Reinitialize()
    {
        Initialize(characterObject);
    }
    
    public IEnumerator FlashRed()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        for (int i = 0; i < 5; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    public void ChangeCurrentStats(CharacterStats  mods)
    {
        currentStats.AddStats(mods);
        OnStatsChanged();
    }

    protected virtual void OnStatsChanged()
    {
        // Custom logic for handling stat changes
    }

    public void ChangeMaxStats(CharacterStats mods)
    {
        characterObject.maxStats.AddStats(mods);
        ChangeCurrentStats(mods);
    }

    //Implementation of IDamageSource
    #region

    public GameObject SourceGameObject => gameObject;
    public float DamageAmount => currentStats.attackDamage;
    public float CriticalChance => currentStats.criticalChance;
    public float CriticalMultiplier => currentStats.criticalMultiplier;
    
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
        bool isCritical = UnityEngine.Random.value < attackerCritChance;
        float baseDamage = isCritical ? incomingAtkDamage * attackerCritDmgCoef : incomingAtkDamage;
        float damageReduction = currentStats.armor / (currentStats.armor + 100);
        return baseDamage * (1 - damageReduction);
    }
    #endregion
    
    public AudioEmitter GetAudioEmitter()
    {
        return audioEmitter;
    }
}

