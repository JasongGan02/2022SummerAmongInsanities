using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterAtlas", menuName = "Atlas/Character Atlas")]
public class CharacterAtlas : ScriptableObject
{
    public CharacterObject villager;
    public CharacterObject player;
    public CharacterObject bat;
    public CharacterObject lady;

    public List<CharacterObject> towerList;
}
