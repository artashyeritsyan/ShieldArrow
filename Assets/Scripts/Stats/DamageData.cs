using UnityEngine;

public interface IDamageable
{
    Team Team { get; }

    void TakeDamage(
        DamageData damageData,
        CoreStats attackerStats);
}

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