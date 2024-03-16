using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileAtlas", menuName = "Atlas/Projectile Atlas")]
public class ProjectileAtlas : ScriptableObject
{
    [Header("Projectiles")]
    public ProjectileObject arrow;
    public ProjectileObject cannonball;
    public ProjectileObject fireball;
    public ProjectileObject icebolt;
    public ProjectileObject rock;
    public ProjectileObject snowball;
}