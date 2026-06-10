using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    // Core Stats
    public float speed;
    public float maxHP;
    public float currentHP;
    public float damage;

    // Special Effects
    public float vampirism;       // % of damage returned as HP
    public float poisonDamage;   // damage over time
    public float fireDamage;     // burn or reflect
    public float iceDamage;      // slow or reflect
    public float lightningDamage; // chain or burst damage

    // Constructor (initialize from base values)
    public PlayerStats(
        float speed,
        float maxHP,
        float damage,
        float vampirism,
        float poisonDamage,
        float fireDamage,
        float iceDamage,
        float lightningDamage)
    {
        this.speed = speed;
        this.maxHP = maxHP;
        this.currentHP = maxHP;
        this.damage = damage;

        this.vampirism = vampirism;
        this.poisonDamage = poisonDamage;
        this.fireDamage = fireDamage;
        this.iceDamage = iceDamage;
        this.lightningDamage = lightningDamage;
    }
}


[CreateAssetMenu(fileName = "PlayerStatsProfile", menuName = "Scriptable Objects/PlayerStatsProfile")]
public class PlayerStatsProfile : ScriptableObject
{
    [SerializeField] PlayerStats stats;
}
