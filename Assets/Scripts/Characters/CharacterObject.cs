using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterObject : BaseObject, IBreakableObject
{
    [Header("Character Basic Attributes") ]
    [SerializeField]
    protected float maxHealth;
    [SerializeField]
    protected float curHealth;
    [SerializeField]
    protected float movementSpeed;
    [SerializeField]
    protected float atkDamage;
    [SerializeField]
    protected float armor;
    [SerializeField]
    protected float magicResist;
    [SerializeField]
    protected float atkSpeed; //每秒攻击几次

    [SerializeField]
    protected Drop[] drops;

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
