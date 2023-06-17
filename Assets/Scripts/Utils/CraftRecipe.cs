using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "CraftRecipe")]
public class CraftRecipe : BaseObject
{
    public CraftRecipeObject[] ingredients;

    
    [CreateAssetMenu(menuName = "CraftRecipeObject")]
    public class CraftRecipeObject : BaseObject
    {
        public BaseObject ICraftableObject;
        public int number;

    }
    
}
