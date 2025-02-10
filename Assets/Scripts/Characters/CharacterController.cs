using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[RequireComponent(typeof(AudioEmitter))]
public abstract class CharacterController : MonoBehaviour, IEffectableController, IPoolableObjectController, IDamageable, IDamageSource, IAudioable
{
    #region Fields

    protected CharacterObject characterObject; // Holds the character's base object
    protected CharacterStats currentStats; // Holds the character's current stats
    protected AudioEmitter audioEmitter;
    protected DamageDisplay damageDisplay;
    protected Animator animator;
    protected string currentAnimationState;
    public event Action OnWeaponStatsChanged;

    #endregion

    #region Properties

    public BaseObject PoolableObject => characterObject;
    public CharacterStats CurrentStats => currentStats;
    public CharacterObject CharacterObject => characterObject;
    public List<Type> Hatred { get; set; } = new List<Type>();
    private float previousHealth;
    public float Health
    {
        get => currentStats.hp;
        private set
        {
            // Clamp the new health value
            float clampedValue = Mathf.Clamp(value, 0, characterObject.maxStats.hp);

            // Calculate the HP change
            float hpChange = clampedValue - currentStats.hp;

            // Update the previous health
            previousHealth = currentStats.hp;

            // Set the new health
            currentStats.hp = clampedValue;

            // Call OnHealthChanged with the HP change amount
            OnHealthChanged(hpChange);
        }
    }

    #endregion

    #region Unity Methods

    protected virtual void Awake()
    {
        audioEmitter = GetComponent<AudioEmitter>();
        animator = GetComponent<Animator>();
        damageDisplay = gameObject.AddComponent<DamageDisplay>();
    }

    protected virtual void Update()
    {
        if (transform.position.y < -400f)
        {
            Die();
        }
    }

    protected virtual void OnEnable()
    {
        EffectEvents.OnEffectApplied += HandleEffectApplied;
    }

    protected virtual void OnDisable()
    {
        EffectEvents.OnEffectApplied -= HandleEffectApplied;
    }

    private void HandleEffectApplied(EffectObject effect, Type targetType)
    {
        // Check if the targetType is assignable from the current type (including derived types)
        if (targetType.IsInstanceOfType(this))
        {
            // Add the appropriate EffectController as defined in the EffectObject
            Type effectControllerType = effect.EffectControllerType; // e.g., returns a subclass of EffectController
            if (effectControllerType != null)
            {
                //Debug.Log($"Adding effect controller: {effectControllerType.Name}");
                EffectController controller = gameObject.AddComponent(effectControllerType) as EffectController;
                controller.Initialize(effect);
            }
            else
            {
                Debug.LogError("EffectControllerType is null.");
            }
        }
    }

    #endregion

    #region Initialization

    public virtual void Initialize(CharacterObject characterObject)
    {
        this.characterObject = characterObject;
        currentStats = (CharacterStats)Activator.CreateInstance(characterObject.baseStats.GetType());
        currentStats.CopyFrom(characterObject.maxStats);
        for (int i = 0; i < characterObject.hatred.Count; i++)
        {
            Hatred.Add(Type.GetType(characterObject.hatred[i].name));
        }
    }

    public virtual void Reinitialize()
    {
        Initialize(characterObject);
    }

    /*
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
    */

    #endregion

    #region Health Management

    public virtual async void ChangeHealth(float amount, IDamageSource damageSource)
    {
        Health -= amount;
        if (Health <= 0)
        {
            Die();
        }
        else if (amount > 0 && gameObject.activeSelf)
        {
            //StartCoroutine(FlashRed());
            GameObject onHitVFX = await AddressablesManager.Instance.InstantiateAsync("Assets/Prefabs/VFX/VFXOnHit.prefab", transform.position, transform.rotation);

            if (onHitVFX != null)
            {
                onHitVFX.name = "OnHitVFX"; // Rename for clarity

                // Apply force over time if the damage source is a MonoBehaviour (has position)
                if (damageSource is MonoBehaviour sourceMono)
                {
                    Vector3 forceDirection = (transform.position - sourceMono.transform.position).normalized; // Direction from damage source
                    float forceAmount = Mathf.Clamp(amount / 10f, 3f, 20f);

                    // Find all ParticleSystem components in the spawned prefab (including children)
                    ParticleSystem[] particleSystems = onHitVFX.GetComponentsInChildren<ParticleSystem>();

                    foreach (var ps in particleSystems)
                    {
                        var psMain = ps.main;

                        if (psMain.simulationSpace == ParticleSystemSimulationSpace.Local) // Apply force only if using world space
                        {
                            var psForceOverLifetime = ps.forceOverLifetime;
                            psForceOverLifetime.enabled = true;
                            psForceOverLifetime.x = new ParticleSystem.MinMaxCurve(forceAmount * forceDirection.x);
                            psForceOverLifetime.y = new ParticleSystem.MinMaxCurve(forceAmount * forceDirection.y);
                            psForceOverLifetime.z = new ParticleSystem.MinMaxCurve(forceAmount * forceDirection.z);
                        }
                    }
                }
            }
        }
    }

    protected virtual void OnHealthChanged(float hpChange)
    {
        // Logic for health change, e.g., update UI
    }

    protected virtual async void Die()
    {
        var deathVFX = await AddressablesManager.Instance.InstantiateAsync("Assets/Prefabs/VFX/VFXDead.prefab", transform.position, transform.rotation);
        PoolManager.Instance.Return(gameObject, characterObject);
        var effects = GetComponents<EffectController>();
        foreach (EffectController effect in effects)
            effect.OnObjectInactive();
        OnObjectReturned(false);
    }

    #endregion

    #region Stats Management

    public void AddCurrentStats(CharacterStats mods)
    {
        currentStats += mods;
        currentStats.hp = Mathf.Clamp(currentStats.hp, Mathf.NegativeInfinity, characterObject.maxStats.hp);
        OnStatsChanged();
    }

    public void MultiplyCurrentStats(CharacterStats mods)
    {
        currentStats *= mods;
        OnStatsChanged();
    }

    public void AddMaxStats(CharacterStats mods)
    {
        characterObject.maxStats += mods;
        AddCurrentStats(mods);
    }

    public void MultiplyMaxStats(CharacterStats mods)
    {
        // Backup the old max stats for reference
        CharacterStats oldMaxStats = characterObject.maxStats.Clone();

        // Apply the multiplication to max stats
        characterObject.maxStats *= mods;

        MultiplyCurrentStats(mods);

        // Keep current health unchanged, but clamp it to the new max health
        currentStats.hp = Mathf.Clamp(previousHealth, 0, characterObject.maxStats.hp);
        OnStatsChanged();
    }

    protected virtual void OnStatsChanged()
    {
        OnWeaponStatsChanged?.Invoke();
    }

    #endregion

    #region Damage Handling

    // Implementation of IDamageSource
    public GameObject SourceGameObject => gameObject;
    public float DamageAmount => currentStats.attackDamage;
    public float CriticalChance => currentStats.criticalChance;
    public float CriticalMultiplier => currentStats.criticalMultiplier;

    public float AttackInterval => currentStats.attackInterval;

    public virtual void ApplyDamage(IDamageable target)
    {
        float damageDealt = target.CalculateDamage(DamageAmount, CriticalChance, CriticalMultiplier);
        target.TakeDamage(damageDealt, this);
    }

    // Implementation of IDamageable
    public virtual void TakeDamage(float amount, IDamageSource damageSource)
    {
        damageDisplay?.ShowDamage(amount, transform, characterObject.maxStats.hp, damageSource);
        ChangeHealth(amount, damageSource);
    }

    public virtual float CalculateDamage(float incomingDamage, float attackerCritChance, float attackerCritMultiplier)
    {
        bool isCritical = UnityEngine.Random.value < attackerCritChance;
        float baseDamage = isCritical ? incomingDamage * attackerCritMultiplier : incomingDamage;
        float damageReduction = currentStats.armor / (currentStats.armor + 100);
        return baseDamage * (1 - damageReduction);
    }

    #endregion

    #region Visual Effects

    private Coroutine flashCoroutine;

    private IEnumerator FlashRed()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) yield break;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRedRoutine(spriteRenderer));
    }

    private IEnumerator FlashRedRoutine(SpriteRenderer spriteRenderer)
    {
        const int flashCount = 5;
        const float flashDuration = 0.2f;
        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }

        flashCoroutine = null;
    }

    #endregion

    #region Pooling

    protected virtual void OnObjectReturned(bool isDestroyedByPlayer)
    {
        var drops = characterObject.GetDroppedGameObjects(isDestroyedByPlayer, transform.position);
    }

    #endregion

    #region Audio

    public AudioEmitter GetAudioEmitter()
    {
        return audioEmitter;
    }

    #endregion

    #region Animator

    protected void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;

        animator.Play(newState);

        currentAnimationState = newState;
    }

    #endregion
}