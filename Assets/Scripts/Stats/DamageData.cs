using UnityEngine;

public class DamageData
{
    public float baseDamage;
    public bool canCrit;

    public float critChance;
    public float critMultiplier = 2f;

    public float armorPenetration;

    public float poisonDamage;
    public float fireDamage;
    public float iceDamage;
    public float lightningDamage;

    public GameObject source;
}