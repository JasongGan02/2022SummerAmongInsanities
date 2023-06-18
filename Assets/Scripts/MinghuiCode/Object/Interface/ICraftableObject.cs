using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICraftableObject
{

    BaseObject[] Recipe { get; set; }


    public void Craft(Inventory inventory);

 

}