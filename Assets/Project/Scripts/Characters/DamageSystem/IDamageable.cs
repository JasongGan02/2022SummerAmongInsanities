using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(float amount, IDamageSource source);
    public float CalculateDamage(float incomingAtkDamage, float attackerCritChance, float attackerCritDmgCoef);

    
}
