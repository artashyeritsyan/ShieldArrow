using System;
using System.Diagnostics;

[Serializable]
public class PlayerStats
{
    public CoreStats core;
    public ArcherStats archer;
    public KnightStats knight;

    public PlayerStats Clone()
    {
        return new PlayerStats
        {
            core = core.Clone(),
            archer = archer.Clone(),
            knight = knight.Clone()
        };
    }
}


[Serializable]
public class CoreStats
{
    public float speed;
    public float maxHP;
    public float currentHP;
    public float damage;
    public float attackSpeed;

    public float lifeSteal = 0;  // from 0 to 1  Initial value will be 0.2f
    public float poisonDamage = 0;
    public float fireDamage = 0;
    public float iceDamage = 0;
    public float lightningDamage = 0;

    public CoreStats Clone()
    {
        return new CoreStats
        {
            speed = this.speed,
            maxHP = this.maxHP,
            currentHP = this.maxHP,
            damage = this.damage,
            attackSpeed = this.attackSpeed,

            lifeSteal = this.lifeSteal,
            poisonDamage = this.poisonDamage,
            fireDamage = this.fireDamage,
            iceDamage = this.iceDamage,
            lightningDamage = this.lightningDamage
        };
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        currentHP = Math.Clamp(currentHP, 0, maxHP);
    }

    public void Heal(float amount)
    {
        currentHP += amount;
        currentHP = Math.Clamp(currentHP, 0, maxHP);
    }

    public void ResetHP()
    {
        currentHP = maxHP;
    }
}

[Serializable]
public class ArcherStats
{
    //public float shootingSpeed;
    public float spread = 1;
    public float ricochetMultiplier = 1;

    public ArcherStats Clone()
    {
        return new ArcherStats
        {
            //shootingSpeed = this.shootingSpeed,
            spread = this.spread,
            ricochetMultiplier = this.ricochetMultiplier
        };
    }
}

[Serializable]
public class KnightStats
{
    public float reflectDamageMultiplier = 1;
    public float shieldSize = 1;
    public float shieldAttackCooldown;

    public KnightStats Clone()
    {
        return new KnightStats
        {
            reflectDamageMultiplier = this.reflectDamageMultiplier,
            shieldSize = this.shieldSize,
            shieldAttackCooldown = this.shieldAttackCooldown
        };
    }
}

