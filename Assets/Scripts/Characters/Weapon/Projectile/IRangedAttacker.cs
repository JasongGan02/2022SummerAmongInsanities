using UnityEngine;

public interface IRangedAttacker
{
    float AttackRange { get; } // Property to get the attack range of the character
    WeaponObject WeaponObject { get; } // Property to get the projectile prefab

    void FireProjectile(GameObject target); // Method to fire a projectile in a given direction
}
