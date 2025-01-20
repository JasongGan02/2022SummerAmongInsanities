using UnityEngine;

public interface IDamageSource
{
    public GameObject? SourceGameObject { get; }
    /*public float DamageAmount { get; }
    public float CriticalChance { get; }
    public float CriticalMultiplier { get; }*/
    public void ApplyDamage(IDamageable target);
}
