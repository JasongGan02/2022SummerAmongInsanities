using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private Dictionary<string, int> inventory = new Dictionary<string, int>();

    public void AddItem(ResourceObject item)
    {
        int value = inventory.GetValueOrDefault(item.name);
        Debug.Log(value);
        inventory.Remove(item.name);
        this.inventory.Add(item.name, value + item.amount);
    }
}
