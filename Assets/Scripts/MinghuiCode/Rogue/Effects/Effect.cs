using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Effects")]
public abstract class Effect : BaseObject
{
    public int cost;
    public int level;
    public bool canRepeat;
    

    //Types of Effects: Spawn items, [permanent: Change of Character Object Stats, duration/permanent Change of Game Stat or special effects/new game mechanics], 
    public abstract void ExecuteEffect(IEffectable character);

    /***
        IEffectableObject:
            - Effect List

        Effect
            
    ***/
}
