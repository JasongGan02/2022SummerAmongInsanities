using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterObject : BaseObject, IBreakableObject
{
    [Header("Character Basic Attributes") ]
    
    public float maxHealth;
    
    public float curHealth;
   
    public float movementSpeed;
    
    public float atkDamage;
    
    public float armor;
    
    public float magicResist;
    
    public float atkSpeed; //每秒攻击几次

   
    public Drop[] drops;


public abstract GameObject GetSpawnedCharacter();

#region
public float HealthPoint
{
get => curHealth;
set => curHealth = value;
}

public Drop[] Drops
{
get => drops;
set => drops = value;
}

public List<GameObject> GetDroppedGameObjects(bool isUserPlaced)
{
List<GameObject> droppedItems = new();
if (isUserPlaced)
{
//droppedItems.Add(GetDroppedGameObject(1));
}
else
{
foreach (Drop drop in Drops)
{
GameObject droppedGameObject = drop.GetDroppedItem();
droppedItems.Add(droppedGameObject);
}   
}
return droppedItems;

}
#endregion

}
