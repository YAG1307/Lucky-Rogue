using UnityEngine;

[CreateAssetMenu(fileName = "Abilities", menuName = "Scriptable Objects/Abilities")]
public class Abilities : ScriptableObject
{
    public string abilityName;
    public string description;
    public int energyCost;
    public int cost;
    public bool isPassive;
}
