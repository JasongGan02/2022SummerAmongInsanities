using System.Collections.Generic;

public interface IEffectableObject
{
    List<EffectObject> Effects { get; set; } 

   void ExecuteEffects();

    /*** 
    To Implement: 

    public List<EffectObject> Effects { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    protected virtual void Awake()
    {
        Effects = new List<EffectObject>();
    }***/

}
