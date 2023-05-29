using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEffectable
{
    public void ApplyEffect(Effect effect);
    public void HandleEffect();
    public void RemoveEffect();
}
